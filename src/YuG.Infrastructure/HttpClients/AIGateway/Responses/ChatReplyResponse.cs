namespace YuG.Infrastructure.HttpClients.AIGateway.Responses;

/// <summary>聊天回复响应。</summary>
public class ChatReplyResponse
{
    /// <summary>AI 回复内容。</summary>
    public string Reply { get; set; } = string.Empty;

    /// <summary>会话 ID，用于后续对话。</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>使用的模型标识。</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Token 用量信息（本轮请求）。</summary>
    public UsageData? Usage { get; set; }

    /// <summary>会话累计输入 Token 数。</summary>
    public long TotalInTokens { get; set; }

    /// <summary>会话累计输出 Token 数。</summary>
    public long TotalOutTokens { get; set; }
}
