using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Middleware;
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

        // 使用 AutoInvokeKernelFunctions 让 SK 内部处理多轮工具调用。
        // SK 内部会保留 reasoning_content 的正确回传，避免 HTTP 400。
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var originalCount = history.Count;
        var results = await chatCompletion.GetChatMessageContentsAsync(history, settings, _kernel, ct);
        var last = results.LastOrDefault();

        // 从 history 中提取 SK 自动执行的工具调用记录
        var toolCallRecords = ExtractToolCallRecords(history, originalCount);

        return new ChatReplyResponse
        {
            Reply = last?.Content ?? string.Empty,
            Model = last?.ModelId ?? _options.DeepSeek.ModelId,
            Usage = ExtractUsage(last),
            ToolCalls = toolCallRecords.Count > 0 ? toolCallRecords : null
        };
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
            var reasoningText = new StringBuilder();
            StreamingChatMessageContent? lastChunk = null;

            // 第一轮流式回复：可能包含文本（直接推 SSE）和/或工具调用
            await foreach (var chunk in chatCompletion.GetStreamingChatMessageContentsAsync(
                history, settings, _kernel, ct))
            {
                lastChunk = chunk;

                // 累积 reasoning_content（DeepSeek 可能通过 metadata 返回）
                if (chunk.Metadata?.TryGetValue("reasoning_content", out var rc) == true && rc is string rcText)
                {
                    reasoningText.Append(rcText);
                }

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

            // 流结束：将累积的 reasoning_content 与所有工具调用 ID 关联
            if (reasoningText.Length > 0)
            {
                foreach (var callId in pendingFunctions.Keys)
                {
                    ReasoningContentHandler.CacheReasoningContent(callId, reasoningText.ToString());
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

                // ⚠️ FunctionResultContent 的 4 参构造函数是 (functionName, pluginName, callId, result)，
                // 全部有默认值。必须使用命名参数，否则会错位。
                history.Add(new ChatMessageContent(
                    AuthorRole.Tool,
                    items: [new FunctionResultContent(functionName: name, callId: callId, result: resultContent)]));
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
                        items: [new FunctionResultContent(callId: msg.ToolCallId ?? string.Empty, result: msg.Content)]));
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

    /// <summary>从 <see cref="ChatHistory"/> 中提取 SK 自动执行的工具调用记录。</summary>
    private static List<ToolCallRecord> ExtractToolCallRecords(ChatHistory history, int startIndex)
    {
        var records = new List<ToolCallRecord>();

        for (var i = startIndex; i < history.Count; i++)
        {
            var msg = history[i];
            if (msg.Role != AuthorRole.Assistant)
                continue;

            foreach (var fc in msg.Items.OfType<FunctionCallContent>())
            {
                // 查找紧随其后的对应 tool 结果
                string resultContent = string.Empty;
                for (var j = i + 1; j < history.Count; j++)
                {
                    var toolResult = history[j].Items.OfType<FunctionResultContent>()
                        .FirstOrDefault(fr => fr.CallId == fc.Id);
                    if (toolResult is not null)
                    {
                        resultContent = toolResult.Result?.ToString() ?? string.Empty;
                        break;
                    }
                }

                records.Add(new ToolCallRecord
                {
                    Id = fc.Id ?? string.Empty,
                    Name = fc.FunctionName,
                    Arguments = SerializeArguments(fc.Arguments),
                    Result = resultContent
                });
            }
        }

        return records;
    }

    #region Usage extraction

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
