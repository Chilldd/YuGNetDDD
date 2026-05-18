namespace YuG.AI.Gateway.Models.Requests;

/// <summary>聊天消息，表示对话中的一条消息。</summary>
public class ChatMessageDto
{
    /// <summary>消息角色：system、user、assistant、tool。</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>消息内容。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>工具调用列表（仅 assistant 角色消息使用）。</summary>
    public List<ToolCallDto>? ToolCalls { get; set; }

    /// <summary>工具调用 ID（仅 tool 角色消息使用，用于关联对应的工具调用）。</summary>
    public string? ToolCallId { get; set; }
}
