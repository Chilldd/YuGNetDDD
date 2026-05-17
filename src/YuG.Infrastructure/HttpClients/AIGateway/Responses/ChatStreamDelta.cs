namespace YuG.Infrastructure.HttpClients.AIGateway.Responses;

/// <summary>流式聊天响应的增量数据。</summary>
public class ChatStreamDelta
{
    /// <summary>增量类型："delta"（文本内容）或 "usage"（用量信息）。</summary>
    public string Type { get; set; } = "delta";

    /// <summary>增量文本内容。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Token 用量信息（仅 Type 为 "usage" 时有效）。</summary>
    public UsageData? Usage { get; set; }

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
