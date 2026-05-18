namespace YuG.Infrastructure.HttpClients.AIGateway.Requests;

/// <summary>聊天消息，表示对话中的一条消息。</summary>
public class ChatMessageDto
{
    /// <summary>消息角色：system、user、assistant、tool。</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>消息内容。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>工具调用列表（仅 assistant 角色消息使用）。</summary>
    public List<ToolCallDto>? ToolCalls { get; set; }

    /// <summary>工具调用 ID（仅 tool 角色消息使用）。</summary>
    public string? ToolCallId { get; set; }
}

/// <summary>工具调用信息 DTO，与 AI.Gateway 保持一致。</summary>
public class ToolCallDto
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>函数名称。</summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>函数参数，JSON 格式。</summary>
    public string Arguments { get; set; } = string.Empty;
}
