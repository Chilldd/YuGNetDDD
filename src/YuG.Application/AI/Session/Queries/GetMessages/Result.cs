namespace YuG.Application.AI.Session.Queries.GetMessages;

/// <summary>消息项。</summary>
public record MessageItem
{
    /// <summary>消息角色（system/user/assistant/tool）。</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>消息内容。</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>消息序号，从 0 开始递增。</summary>
    public int SequenceNumber { get; init; }

    /// <summary>Token 数（可选）。</summary>
    public int? TokenCount { get; init; }

    /// <summary>创建时间（UTC）。</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>工具调用 ID（tool 角色消息）。</summary>
    public string? ToolCallId { get; init; }

    /// <summary>工具调用列表 JSON（assistant 消息带 tool_calls）。</summary>
    public string? ToolCalls { get; init; }
}
