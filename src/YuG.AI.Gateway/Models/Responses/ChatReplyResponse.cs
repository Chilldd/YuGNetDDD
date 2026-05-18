namespace YuG.AI.Gateway.Models.Responses;

/// <summary>聊天回复响应。</summary>
public class ChatReplyResponse
{
    /// <summary>AI 回复内容。</summary>
    public string Reply { get; set; } = string.Empty;

    /// <summary>使用的模型标识。</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Token 用量信息（本轮请求）。</summary>
    public UsageData? Usage { get; set; }

    /// <summary>工具调用记录（非流式模式下，AI 调用过的工具列表）。</summary>
    public List<ToolCallRecord>? ToolCalls { get; set; }
}

/// <summary>Token 用量数据。</summary>
public class UsageData
{
    /// <summary>输入 Token 数。</summary>
    public int InTokens { get; set; }

    /// <summary>输出 Token 数。</summary>
    public int OutTokens { get; set; }

    /// <summary>总 Token 数。</summary>
    public int TotalTokens { get; set; }

    /// <summary>缓存命中 Token 数（DeepSeek 等 Provider 支持）。</summary>
    public int? PromptCacheHitTokens { get; set; }

    /// <summary>缓存未命中 Token 数（DeepSeek 等 Provider 支持）。</summary>
    public int? PromptCacheMissTokens { get; set; }
}
