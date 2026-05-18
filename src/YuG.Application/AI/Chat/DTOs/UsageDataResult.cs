namespace YuG.Application.AI.Chat.DTOs;

/// <summary>Token 用量数据。</summary>
public record UsageDataResult
{
    /// <summary>输入 Token 数。</summary>
    public int InTokens { get; init; }

    /// <summary>输出 Token 数。</summary>
    public int OutTokens { get; init; }

    /// <summary>总 Token 数。</summary>
    public int TotalTokens { get; init; }

    /// <summary>缓存命中 Token 数。</summary>
    public int? PromptCacheHitTokens { get; init; }

    /// <summary>缓存未命中 Token 数。</summary>
    public int? PromptCacheMissTokens { get; init; }
}
