namespace YuG.AI.Gateway.Models.Responses;

/// <summary>流式聊天响应的增量数据。</summary>
public class ChatStreamDelta
{
    /// <summary>增量类型，固定为 "delta"。</summary>
    public string Type { get; set; } = "delta";

    /// <summary>增量文本内容。</summary>
    public string Content { get; set; } = string.Empty;

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
