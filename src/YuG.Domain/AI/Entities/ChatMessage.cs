namespace YuG.Domain.AI.Entities;

/// <summary>聊天消息值对象。</summary>
public class ChatMessage
{
    /// <summary>消息角色（system、user、assistant）。</summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>消息内容。</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>消息序号，从 0 开始递增。</summary>
    public int SequenceNumber { get; private set; }

    /// <summary>Token 数（可选）。</summary>
    public int? TokenCount { get; private set; }

    /// <summary>创建时间（UTC）。</summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>用于 ORM。</summary>
    private ChatMessage()
    {
    }

    /// <summary>创建聊天消息。</summary>
    /// <param name="role">消息角色</param>
    /// <param name="content">消息内容</param>
    /// <param name="sequenceNumber">序号</param>
    /// <param name="tokenCount">Token 数（可选）</param>
    public ChatMessage(string role, string content, int sequenceNumber, int? tokenCount = null)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("消息角色不能为空", nameof(role));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("消息内容不能为空", nameof(content));

        Role = role;
        Content = content;
        SequenceNumber = sequenceNumber;
        TokenCount = tokenCount;
    }
}
