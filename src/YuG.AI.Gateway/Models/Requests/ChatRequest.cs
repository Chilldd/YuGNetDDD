namespace YuG.AI.Gateway.Models.Requests;

/// <summary>聊天请求，包含用户标识和完整消息历史。</summary>
public class ChatRequest
{
    /// <summary>用户标识。</summary>
    public long? UserId { get; set; }

    /// <summary>完整消息历史列表。</summary>
    public List<ChatMessageDto> Messages { get; set; } = [];
}
