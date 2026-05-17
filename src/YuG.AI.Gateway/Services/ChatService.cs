using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Models.Responses;

namespace YuG.AI.Gateway.Services;

/// <summary>聊天服务实现，使用 Semantic Kernel 的 <see cref="IChatCompletionService"/> 和 <see cref="ChatHistory"/> 管理对话。</summary>
public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly AiOptions _options;

    /// <summary>初始化 <see cref="ChatService"/> 实例。</summary>
    /// <param name="kernel">Semantic Kernel 实例</param>
    /// <param name="options">AI 配置选项</param>
    public ChatService(Kernel kernel, IOptions<AiOptions> options)
    {
        _kernel = kernel;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<ChatReplyResponse> ChatWithHistoryAsync(
        ChatHistory history, string question, CancellationToken ct = default)
    {
        history.AddUserMessage(question);

        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        var results = await chatCompletion.GetChatMessageContentsAsync(history, null, null, ct);

        var last = results.LastOrDefault();
        var reply = last?.Content ?? string.Empty;

        if (reply.Length > 0)
            history.AddAssistantMessage(reply);

        return new ChatReplyResponse
        {
            Reply = reply,
            Model = last?.ModelId ?? _options.DeepSeek.ModelId,
            Usage = ExtractUsage(last),
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDelta> ChatStreamWithHistoryAsync(
        ChatHistory history, string question, [EnumeratorCancellation] CancellationToken ct = default)
    {
        history.AddUserMessage(question);

        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        var fullReply = new StringBuilder();
        StreamingChatMessageContent? lastChunk = null;

        await foreach (var chunk in chatCompletion.GetStreamingChatMessageContentsAsync(history, null, null, ct))
        {
            lastChunk = chunk;
            if (chunk.Content is { Length: > 0 } text)
            {
                fullReply.Append(text);
                yield return new ChatStreamDelta(text);
            }
        }

        var usage = ExtractUsage(lastChunk);
        if (usage is not null)
            yield return new ChatStreamDelta { Type = "usage", Usage = usage };

        if (fullReply.Length > 0)
            history.AddAssistantMessage(fullReply.ToString());
    }

    /// <summary>从 SK 响应中提取 Token 用量。</summary>
    private static UsageData? ExtractUsage(ChatMessageContent? message)
    {
        if (message is null) return null;

        // 策略 1：metadata["Usage"]
        if (message.Metadata?.TryGetValue("Usage", out var usageObj) == true && usageObj is not null)
            return ParseUsageObject(usageObj);

        // 策略 2：InnerContent 上找 Usage 属性
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

    /// <summary>从 SK 流式响应中提取 Token 用量。</summary>
    private static UsageData? ExtractUsage(StreamingChatMessageContent? chunk)
    {
        if (chunk is null) return null;

        // 策略 1：metadata["Usage"]
        if (chunk.Metadata?.TryGetValue("Usage", out var usageObj) == true && usageObj is not null)
            return ParseUsageObject(usageObj);

        // 策略 2：InnerContent 上找 Usage
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
}
