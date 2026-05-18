using System.Text.Json.Serialization;

namespace YuG.Infrastructure.HttpClients.AIGateway.Responses;

/// <summary>流式聊天响应的增量数据。</summary>
public class ChatStreamDelta
{
    /// <summary>增量类型："delta"（文本内容）、"usage"（用量）、"tool_call"（工具调用）、"tool_result"（工具结果）。</summary>
    public string Type { get; set; } = "delta";

    /// <summary>增量文本内容。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Token 用量信息（仅 Type 为 "usage" 时有效）。</summary>
    public UsageData? Usage { get; set; }

    /// <summary>工具调用信息（仅 Type 为 "tool_call" 时有效）。</summary>
    [JsonPropertyName("tool_call")]
    public ToolCallDelta? ToolCall { get; set; }

    /// <summary>工具调用结果（仅 Type 为 "tool_result" 时有效）。</summary>
    [JsonPropertyName("tool_result")]
    public ToolCallResultDelta? ToolResult { get; set; }

    /// <summary>初始化 <see cref="ChatStreamDelta"/> 实例。</summary>
    public ChatStreamDelta()
    {
    }

    /// <summary>初始化 <see cref="ChatStreamDelta"/> 实例并指定内容。</summary>
    /// <param name="content">增量文本内容</param>
    public ChatStreamDelta(string content)
    {
        Content = content;
    }
}

/// <summary>SSE tool_call 事件数据。</summary>
public class ToolCallDelta
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>函数名称（内部标识）。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>函数友好名称（Description 特性值），用于 UI 展示。</summary>
    public string? DisplayName { get; set; }

    /// <summary>函数参数 JSON。</summary>
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>SSE tool_result 事件数据。</summary>
public class ToolCallResultDelta
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>函数名称（内部标识）。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>函数友好名称（Description 特性值），用于 UI 展示。</summary>
    public string? DisplayName { get; set; }

    /// <summary>工具执行结果。</summary>
    public string Content { get; set; } = string.Empty;
}
