namespace YuG.AI.Gateway.Models.Requests;

/// <summary>聊天消息，表示对话中的一条消息。</summary>
public class ChatMessageDto
{
    /// <summary>消息角色：system、user、assistant。</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>消息内容。</summary>
    public string Content { get; set; } = string.Empty;
}
