using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Models.Requests;
using YuG.AI.Gateway.Models.Responses;

namespace YuG.AI.Gateway.Services;

/// <summary>聊天服务实现，使用 Semantic Kernel 处理消息补全与工具调用。</summary>
public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly AiOptions _options;

    /// <summary>初始化 <see cref="ChatService"/> 实例。</summary>
    /// <param name="kernel">Semantic Kernel 实例（已注册插件）</param>
    /// <param name="options">AI 配置选项</param>
    public ChatService(Kernel kernel, IOptions<AiOptions> options)
    {
        _kernel = kernel;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<ChatReplyResponse> ChatAsync(List<ChatMessageDto> messages, CancellationToken ct = default)
    {
        var history = BuildChatHistory(messages);
        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
        };

        var toolCallRecords = new List<ToolCallRecord>();

        for (var round = 0; round < _options.MaxToolCallRounds; round++)
        {
            var results = await chatCompletion.GetChatMessageContentsAsync(history, settings, _kernel, ct);
            var message = results.LastOrDefault();
            if (message is null) continue;

            var functionCalls = message.Items.OfType<FunctionCallContent>().ToList();
            if (functionCalls.Count == 0)
            {
                return new ChatReplyResponse
                {
                    Reply = message.Content ?? string.Empty,
                    Model = message.ModelId ?? _options.DeepSeek.ModelId,
                    Usage = ExtractUsage(message),
                    ToolCalls = toolCallRecords.Count > 0 ? toolCallRecords : null
                };
            }

            // 将 LLM 返回的 tool_calls 加入历史
            history.Add(message);

            // 依次执行每个函数调用并记录结果
            foreach (var fc in functionCalls)
            {
                var resultContent = await ExecuteFunctionCall(fc, ct);

                toolCallRecords.Add(new ToolCallRecord
                {
                    Id = fc.Id ?? string.Empty,
                    Name = fc.FunctionName,
                    Arguments = SerializeArguments(fc.Arguments),
                    Result = resultContent
                });

                history.Add(new ChatMessageContent(
                    AuthorRole.Tool,
                    items: [new FunctionResultContent(fc.Id ?? string.Empty, fc.FunctionName, resultContent)]));
            }
        }

        throw new InvalidOperationException($"工具调用超出最大轮数 ({_options.MaxToolCallRounds})，请检查工具实现是否有误。");
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDelta> ChatStreamAsync(
        List<ChatMessageDto> messages, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var history = BuildChatHistory(messages);
        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
        };

        for (var round = 0; round < _options.MaxToolCallRounds; round++)
        {
            var pendingFunctions = new Dictionary<string, (string Name, StringBuilder ArgsBuilder)>();
            StreamingChatMessageContent? lastChunk = null;

            // 第一轮流式回复：可能包含文本（直接推 SSE）和/或工具调用
            await foreach (var chunk in chatCompletion.GetStreamingChatMessageContentsAsync(
                history, settings, _kernel, ct))
            {
                lastChunk = chunk;

                // 追踪流式工具调用更新（跨 chunks 累积 arguments）
                foreach (var item in chunk.Items)
                {
                    if (item is StreamingFunctionCallUpdateContent fc)
                    {
                        var callId = fc.CallId ?? Guid.NewGuid().ToString();
                        if (!pendingFunctions.TryGetValue(callId, out var existing))
                        {
                            existing = (fc.Name ?? string.Empty, new StringBuilder());
                            pendingFunctions[callId] = existing;
                        }
                        if (fc.Arguments is { Length: > 0 })
                        {
                            existing.ArgsBuilder.Append(fc.Arguments);
                        }
                    }
                }

                // 文本内容实时推 SSE
                if (chunk.Content?.Length > 0)
                {
                    yield return new ChatStreamDelta(chunk.Content);
                }
            }

            // 没有工具调用 → 流结束，发送用量后退出
            if (pendingFunctions.Count == 0)
            {
                var usage = ExtractUsage(lastChunk);
                if (usage is not null)
                    yield return new ChatStreamDelta { Type = "usage", Usage = usage };
                yield break;
            }

            // 将 LLM 的 tool_calls 加入历史
            var callItems = new ChatMessageContentItemCollection();
            foreach (var (callId, (name, argsBuilder)) in pendingFunctions)
            {
                var args = ParseArguments(argsBuilder.ToString());
                callItems.Add(new FunctionCallContent(name, id: callId, arguments: args));
            }
            history.Add(new ChatMessageContent(AuthorRole.Assistant, items: callItems));

            // 依次执行，发送 tool_call / tool_result SSE 事件
            foreach (var (callId, (name, argsBuilder)) in pendingFunctions)
            {
                yield return new ChatStreamDelta
                {
                    Type = "tool_call",
                    ToolCall = new ToolCallDelta
                    {
                        Id = callId,
                        Name = name,
                        Arguments = argsBuilder.ToString()
                    }
                };

                var resultContent = await ExecuteFunctionCall(name, argsBuilder.ToString(), ct);

                yield return new ChatStreamDelta
                {
                    Type = "tool_result",
                    ToolResult = new ToolCallResultDelta
                    {
                        Id = callId,
                        Name = name,
                        Content = resultContent
                    }
                };

                history.Add(new ChatMessageContent(
                    AuthorRole.Tool,
                    items: [new FunctionResultContent(callId, name, resultContent)]));
            }

            // 继续下一轮，LLM 将基于工具结果生成回复
        }
    }

    /// <summary>通过 Kernel 执行指定的函数调用。</summary>
    private async Task<string> ExecuteFunctionCall(FunctionCallContent fc, CancellationToken ct)
    {
        return await ExecuteFunctionCall(fc.FunctionName, args: fc.Arguments, ct);
    }

    /// <summary>通过 Kernel 执行指定名称的函数。</summary>
    private async Task<string> ExecuteFunctionCall(string name, KernelArguments? args, CancellationToken ct)
    {
        try
        {
            var function = _kernel.Plugins.GetFunction(null, name);
            if (function is null)
                return $"Error: Function '{name}' not found";

            var result = await _kernel.InvokeAsync(function, args ?? [], ct);
            return result.GetValue<object>()?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>通过 Kernel 执行指定名称的函数（JSON 参数）。</summary>
    private async Task<string> ExecuteFunctionCall(string name, string? argsJson, CancellationToken ct)
    {
        var args = ParseArguments(argsJson);
        return await ExecuteFunctionCall(name, args, ct);
    }

    /// <summary>将 <see cref="ChatMessageDto"/> 列表转换为 SK 的 <see cref="ChatHistory"/>。</summary>
    private static ChatHistory BuildChatHistory(List<ChatMessageDto> messages)
    {
        var history = new ChatHistory();
        foreach (var msg in messages)
        {
            switch (msg.Role.ToLowerInvariant())
            {
                case "system":
                    history.AddSystemMessage(msg.Content);
                    break;
                case "user":
                    history.AddUserMessage(msg.Content);
                    break;
                case "assistant":
                    if (msg.ToolCalls?.Count > 0)
                    {
                        var items = new ChatMessageContentItemCollection();
                        if (!string.IsNullOrEmpty(msg.Content))
                            items.Add(new TextContent(msg.Content));
                        foreach (var tc in msg.ToolCalls)
                        {
                            items.Add(new FunctionCallContent(tc.FunctionName, id: tc.Id, arguments: ParseArguments(tc.Arguments)));
                        }
                        history.Add(new ChatMessageContent(AuthorRole.Assistant, items: items));
                    }
                    else
                    {
                        history.AddAssistantMessage(msg.Content);
                    }
                    break;
                case "tool":
                    history.Add(new ChatMessageContent(
                        AuthorRole.Tool,
                        items: [new FunctionResultContent(msg.ToolCallId ?? string.Empty, string.Empty, msg.Content)]));
                    break;
            }
        }
        return history;
    }

    /// <summary>将 JSON 参数字符串解析为 <see cref="KernelArguments"/>。</summary>
    private static KernelArguments ParseArguments(string? json)
    {
        if (string.IsNullOrEmpty(json) || json == "{}")
            return [];

        var parsed = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
        if (parsed is null || parsed.Count == 0)
            return [];

        var args = new KernelArguments();
        foreach (var kvp in parsed)
        {
            args[kvp.Key] = kvp.Value;
        }
        return args;
    }

    /// <summary>将 <see cref="KernelArguments"/> 序列化为 JSON 字符串。</summary>
    private static string SerializeArguments(KernelArguments? args)
    {
        if (args is null || args.Count == 0)
            return "{}";

        return JsonSerializer.Serialize(args.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    #region Usage extraction (原文保留)

    private static UsageData? ExtractUsage(ChatMessageContent? message)
    {
        if (message is null) return null;

        if (message.Metadata?.TryGetValue("Usage", out var usageObj) == true && usageObj is not null)
            return ParseUsageObject(usageObj);

        if (message.InnerContent is not null)
        {
            var usageProp = message.InnerContent.GetType().GetProperty("Usage");
            if (usageProp is not null)
            {
                var usage = usageProp.GetValue(message.InnerContent);
                if (usage is not null)
                    return ParseUsageObject(usage);
            }
        }

        return null;
    }

    private static UsageData? ExtractUsage(StreamingChatMessageContent? chunk)
    {
        if (chunk is null) return null;

        if (chunk.Metadata?.TryGetValue("Usage", out var usageObj) == true && usageObj is not null)
            return ParseUsageObject(usageObj);

        if (chunk.InnerContent is not null)
        {
            var usageProp = chunk.InnerContent.GetType().GetProperty("Usage");
            if (usageProp is not null)
            {
                var usage = usageProp.GetValue(chunk.InnerContent);
                if (usage is not null)
                    return ParseUsageObject(usage);
            }
        }

        return null;
    }

    private static UsageData ParseUsageObject(object obj)
    {
        static int GetInt(object target, string name1, string name2)
        {
            var prop = target.GetType().GetProperty(name1, BindingFlags.Public | BindingFlags.Instance)
                     ?? target.GetType().GetProperty(name2, BindingFlags.Public | BindingFlags.Instance);
            return prop is not null ? (int)(prop.GetValue(target) ?? 0) : 0;
        }

        static int? GetNestedInt(object target, string outerProp, string innerProp1, string innerProp2)
        {
            var outer = target.GetType().GetProperty(outerProp, BindingFlags.Public | BindingFlags.Instance);
            var inner = outer?.GetValue(target);
            return inner is not null ? GetInt(inner, innerProp1, innerProp2) : null;
        }

        var hit = GetNestedInt(obj, "InputTokenDetails", "CachedTokenCount", "CachedTokens")
               ?? GetInt(obj, "PromptCacheHitTokens", "prompt_cache_hit_tokens");
        var miss = GetInt(obj, "PromptCacheMissTokens", "prompt_cache_miss_tokens");

        return new UsageData
        {
            InTokens = GetInt(obj, "InputTokenCount", "InputTokens"),
            OutTokens = GetInt(obj, "OutputTokenCount", "OutputTokens"),
            TotalTokens = GetInt(obj, "TotalTokenCount", "TotalTokens"),
            PromptCacheHitTokens = hit,
            PromptCacheMissTokens = miss,
        };
    }

    #endregion
}
