namespace YuG.AI.Gateway.Models.Requests;

/// <summary>工具调用信息，表示 LLM 请求调用某个工具。</summary>
public class ToolCallDto
{
    /// <summary>工具调用 ID，用于匹配调用与结果。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>函数名称。</summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>函数参数，JSON 格式。</summary>
    public string Arguments { get; set; } = string.Empty;
}
