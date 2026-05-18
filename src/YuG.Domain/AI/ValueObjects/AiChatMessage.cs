namespace YuG.Domain.AI.ValueObjects;

/// <summary>聊天消息值对象。</summary>
public record AiChatMessage
{
    /// <summary>消息角色（system、user、assistant、tool）。</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>消息内容。对于带 tool_calls 的 assistant 消息可为空。</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>消息序号，从 0 开始递增。</summary>
    public int SequenceNumber { get; init; }

    /// <summary>Token 数（可选）。</summary>
    public int? TokenCount { get; init; }

    /// <summary>创建时间（UTC）。</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>工具调用 ID（仅 tool 角色消息使用）。</summary>
    public string? ToolCallId { get; init; }

    /// <summary>工具调用列表 JSON（仅 assistant 角色且包含 tool_calls 时使用）。</summary>
    public string? ToolCalls { get; init; }

    /// <summary>用于 ORM。</summary>
    private AiChatMessage()
    {
    }

    /// <summary>创建聊天消息。</summary>
    /// <param name="role">消息角色</param>
    /// <param name="content">消息内容（tool_calls 的 assistant 消息可传空字符串）</param>
    /// <param name="sequenceNumber">序号</param>
    /// <param name="tokenCount">Token 数（可选）</param>
    /// <param name="toolCallId">工具调用 ID（tool 角色）</param>
    /// <param name="toolCalls">工具调用列表 JSON（assistant 角色带 tool_calls）</param>
    public AiChatMessage(
        string role,
        string content,
        int sequenceNumber,
        int? tokenCount = null,
        string? toolCallId = null,
        string? toolCalls = null) : this()
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("消息角色不能为空", nameof(role));

        Role = role;
        Content = content;
        SequenceNumber = sequenceNumber;
        TokenCount = tokenCount;
        ToolCallId = toolCallId;
        ToolCalls = toolCalls;
    }
}

/// <summary>工具调用信息，序列化为 JSON 存储在 AiChatMessage.ToolCalls 中。</summary>
public record ToolCallInfo
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>函数名称。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>函数参数 JSON。</summary>
    public string Arguments { get; init; } = string.Empty;
}
