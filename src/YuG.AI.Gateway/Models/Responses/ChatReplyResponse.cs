namespace YuG.AI.Gateway.Models.Responses;

/// <summary>聊天回复响应。</summary>
public class ChatReplyResponse
{
    /// <summary>AI 回复内容。</summary>
    public string Reply { get; set; } = string.Empty;

    /// <summary>会话 ID，用于后续对话。</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>使用的模型标识。</summary>
    public string Model { get; set; } = string.Empty;
}
