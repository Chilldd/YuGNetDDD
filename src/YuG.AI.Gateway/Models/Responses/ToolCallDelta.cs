namespace YuG.AI.Gateway.Models.Responses;

/// <summary>流式工具调用事件（SSE type="tool_call"）。</summary>
public class ToolCallDelta
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>函数名称。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>函数参数，JSON 格式。</summary>
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>流式工具调用结果事件（SSE type="tool_result"）。</summary>
public class ToolCallResultDelta
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>函数名称。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>工具执行结果内容。</summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>非流式响应中的工具调用记录。</summary>
public class ToolCallRecord
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>函数名称。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>函数参数，JSON 格式。</summary>
    public string Arguments { get; set; } = string.Empty;

    /// <summary>工具执行结果。</summary>
    public string Result { get; set; } = string.Empty;
}
