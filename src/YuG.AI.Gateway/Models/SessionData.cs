using Microsoft.SemanticKernel.ChatCompletion;

namespace YuG.AI.Gateway.Models;

/// <summary>会话数据，包含对话历史和统计信息。</summary>
public class SessionData
{
    /// <summary>对话历史。</summary>
    public ChatHistory History { get; } = new();

    /// <summary>会话创建时间。</summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>最后活动时间。</summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>累计输入 Token 数。</summary>
    public long TotalInTokens { get; set; }

    /// <summary>累计输出 Token 数。</summary>
    public long TotalOutTokens { get; set; }
}
