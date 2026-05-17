namespace YuG.Application.AI.Chat.Common;

/// <summary>聊天回复结果。</summary>
public record ChatReplyResult
{
    /// <summary>AI 回复内容。</summary>
    public string Reply { get; init; } = string.Empty;

    /// <summary>会话 ID，用于后续对话。</summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>使用的模型标识。</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Token 用量信息（本轮请求）。</summary>
    public UsageDataResult? Usage { get; init; }

    /// <summary>会话累计输入 Token 数。</summary>
    public long TotalInTokens { get; init; }

    /// <summary>会话累计输出 Token 数。</summary>
    public long TotalOutTokens { get; init; }
}
