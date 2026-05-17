namespace YuG.Application.AI.Chat.Common;

/// <summary>流式聊天响应的增量数据。</summary>
public record ChatStreamDeltaResult
{
    /// <summary>增量类型："delta"（文本内容）或 "usage"（用量信息）。</summary>
    public string Type { get; init; } = "delta";

    /// <summary>增量文本内容。</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Token 用量信息（仅 Type 为 "usage" 时有效）。</summary>
    public UsageDataResult? Usage { get; init; }
}
