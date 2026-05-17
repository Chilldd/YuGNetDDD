namespace YuG.AI.Gateway.Models.Requests;

/// <summary>聊天请求，包含用户输入消息。</summary>
public class ChatRequest
{
    /// <summary>用户输入消息内容。</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>会话 ID。为空则服务端自动创建新会话。</summary>
    public string? SessionId { get; set; }
}
