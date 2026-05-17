namespace YuG.Infrastructure.HttpClients.AIGateway.Responses;

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
