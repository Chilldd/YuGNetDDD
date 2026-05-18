namespace YuG.Application.AI.Chat.DTOs;

/// <summary>聊天回复结果。</summary>
public record ChatReplyResult
{
    /// <summary>AI 回复内容。</summary>
    public string Reply { get; init; } = string.Empty;

    /// <summary>会话 ID，用于后续对话。</summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>使用的模型标识。</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Token 用量信息（本轮请求）。</summary>
    public UsageDataResult? Usage { get; init; }

    /// <summary>会话累计输入 Token 数。</summary>
    public long TotalInTokens { get; init; }

    /// <summary>会话累计输出 Token 数。</summary>
    public long TotalOutTokens { get; init; }

    /// <summary>工具调用记录。</summary>
    public List<ToolCallRecordResult>? ToolCalls { get; init; }
}

/// <summary>工具调用记录 DTO。</summary>
public record ToolCallRecordResult
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>函数名称（内部标识）。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>函数友好名称（Description 特性值），用于 UI 展示。</summary>
    public string? DisplayName { get; init; }

    /// <summary>函数参数 JSON。</summary>
    public string Arguments { get; init; } = string.Empty;

    /// <summary>工具执行结果。</summary>
    public string Result { get; init; } = string.Empty;
}
