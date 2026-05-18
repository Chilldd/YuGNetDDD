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

        var results = await chatCompletion.GetChatMessageContentsAsync(history, settings, _kernel, ct);
        var last = results.LastOrDefault();

        // 从结果中提取工具调用
        var toolCallRecords = last?.Items.OfType<FunctionCallContent>()
            .Select(fc => new ToolCallRecord
            {
                Id = fc.Id ?? string.Empty,
                Name = fc.FunctionName ?? string.Empty,
                DisplayName = GetFunctionDisplayName(fc.FunctionName ?? string.Empty),
                Arguments = SerializeArguments(fc.Arguments),
            }).ToList();

        return new ChatReplyResponse
        {
            Reply = last?.Content ?? string.Empty,
            Model = last?.ModelId ?? _options.DeepSeek.ModelId,
            Usage = ExtractUsage(last),
            ToolCalls = toolCallRecords?.Count > 0 ? toolCallRecords : null
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
            // 按 FunctionCallIndex 分组追踪流式工具调用（CallId 在后续 chunks 中可能为空）
            var pendingFunctions = new Dictionary<int, PendingFunction>();
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
                        if (!pendingFunctions.TryGetValue(fc.FunctionCallIndex, out var pf))
                        {
                            pf = new PendingFunction();
                            pendingFunctions[fc.FunctionCallIndex] = pf;
                        }

                        // 合并来自不同 chunks 的属性
                        if (!string.IsNullOrEmpty(fc.CallId))
                            pf.CallId = fc.CallId;
                        if (!string.IsNullOrEmpty(fc.Name))
                            pf.Name = fc.Name;
                        if (fc.Arguments is { Length: > 0 })
                            pf.Arguments.Append(fc.Arguments);
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

            // 将 LLM 的 tool_calls 加入历史（跳过 Name 为空的无效条目）
            var callItems = new ChatMessageContentItemCollection();
            foreach (var pf in pendingFunctions.Values)
            {
                if (string.IsNullOrEmpty(pf.Name))
                    continue;
                var args = ParseArguments(pf.Arguments.ToString());
                callItems.Add(new FunctionCallContent(pf.Name, id: pf.CallId, arguments: args));
            }
            history.Add(new ChatMessageContent(AuthorRole.Assistant, items: callItems));

            // 依次执行，发送 tool_call / tool_result SSE 事件
            foreach (var pf in pendingFunctions.Values)
            {
                if (string.IsNullOrEmpty(pf.Name))
                    continue;

                yield return new ChatStreamDelta
                {
                    Type = "tool_call",
                    ToolCall = new ToolCallDelta
                    {
                        Id = pf.CallId,
                        Name = pf.Name,
                        DisplayName = GetFunctionDisplayName(pf.Name),
                        Arguments = pf.Arguments.ToString()
                    }
                };

                var resultContent = await ExecuteFunctionCall(pf.Name, pf.Arguments.ToString(), ct);

                yield return new ChatStreamDelta
                {
                    Type = "tool_result",
                    ToolResult = new ToolCallResultDelta
                    {
                        Id = pf.CallId,
                        Name = pf.Name,
                        DisplayName = GetFunctionDisplayName(pf.Name),
                        Content = resultContent
                    }
                };

                // ⚠️ FunctionResultContent 的 4 参构造函数是 (functionName, pluginName, callId, result)，
                // 全部有默认值。必须使用命名参数，否则会错位。
                history.Add(new ChatMessageContent(
                    AuthorRole.Tool,
                    items: [new FunctionResultContent(functionName: pf.Name, callId: pf.CallId, result: resultContent)]));
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
            // LLM 返回的全名格式为 "{pluginName}-{functionName}"，需要解析后查找
            var (pluginName, functionName) = ParsePluginFunctionName(name);
            var function = _kernel.Plugins.GetFunction(pluginName, functionName);
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

    /// <summary>解析 LLM 返回的全名"{pluginName}-{functionName}"，无分隔符时 pluginName 为 null。</summary>
    private static (string? pluginName, string functionName) ParsePluginFunctionName(string name)
    {
        var separatorIndex = name.IndexOf('-');
        return separatorIndex > 0
            ? (name[..separatorIndex], name[(separatorIndex + 1)..])
            : (null, name);
    }

    /// <summary>获取函数的 Description 特性中的友好名称，找不到时回退为原始名称。</summary>
    private string GetFunctionDisplayName(string name)
    {
        var (pluginName, functionName) = ParsePluginFunctionName(name);
        try
        {
            var function = _kernel.Plugins.GetFunction(pluginName, functionName);
            var description = function.Metadata.Description;
            return string.IsNullOrEmpty(description) ? name : description;
        }
        catch
        {
            return name;
        }
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

    /// <summary>流式工具调用追踪辅助类，按 FunctionCallIndex 分组跨 chunks 合并数据。</summary>
    private sealed class PendingFunction
    {
        /// <summary>工具调用 ID。</summary>
        public string CallId { get; set; } = string.Empty;

        /// <summary>函数名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>累积的参数 JSON。</summary>
        public StringBuilder Arguments { get; } = new();
    }
}
