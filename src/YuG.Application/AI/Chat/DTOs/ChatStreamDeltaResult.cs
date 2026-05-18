namespace YuG.Application.AI.Chat.DTOs;

/// <summary>流式聊天响应的增量数据。</summary>
public record ChatStreamDeltaResult
{
    /// <summary>增量类型："delta"、"usage"、"tool_call"、"tool_result"、"done"。</summary>
    public string Type { get; init; } = "delta";

    /// <summary>增量文本内容。</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Token 用量信息（仅 Type 为 "usage" 时有效）。</summary>
    public UsageDataResult? Usage { get; init; }

    /// <summary>工具调用信息（仅 Type 为 "tool_call" 时有效）。</summary>
    public ToolCallDeltaResult? ToolCall { get; init; }

    /// <summary>工具调用结果（仅 Type 为 "tool_result" 时有效）。</summary>
    public ToolCallResultDeltaResult? ToolResult { get; init; }

    /// <summary>会话 ID（仅 Type 为 "done" 时有效，服务端创建新会话后回传）。</summary>
    public string? SessionId { get; init; }
}

/// <summary>工具调用事件 DTO。</summary>
public record ToolCallDeltaResult
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>函数名称（内部标识）。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>函数友好名称（Description 特性值），用于 UI 展示。</summary>
    public string? DisplayName { get; init; }

    /// <summary>函数参数 JSON。</summary>
    public string Arguments { get; init; } = string.Empty;
}

/// <summary>工具调用结果事件 DTO。</summary>
public record ToolCallResultDeltaResult
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>函数名称（内部标识）。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>函数友好名称（Description 特性值），用于 UI 展示。</summary>
    public string? DisplayName { get; init; }

    /// <summary>工具执行结果。</summary>
    public string Content { get; init; } = string.Empty;
}
