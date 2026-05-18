namespace YuG.Infrastructure.HttpClients.AIGateway.Responses;

/// <summary>聊天回复响应。</summary>
public class ChatReplyResponse
{
    /// <summary>AI 回复内容。</summary>
    public string Reply { get; set; } = string.Empty;

    /// <summary>使用的模型标识。</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Token 用量信息（本轮请求）。</summary>
    public UsageData? Usage { get; set; }

    /// <summary>工具调用记录。</summary>
    public List<ToolCallRecord>? ToolCalls { get; set; }
}

/// <summary>工具调用记录。</summary>
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
