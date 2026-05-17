namespace YuG.AI.Gateway.Models.Requests;

/// <summary>用户提问请求。</summary>
public class ChatQuestionRequest
{
    /// <summary>用户提问内容。</summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>会话 ID。为空则服务端自动创建新会话。</summary>
    public string? SessionId { get; set; }
}
