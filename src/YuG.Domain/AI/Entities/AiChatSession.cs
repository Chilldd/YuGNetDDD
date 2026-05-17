using YuG.Domain.Common;
using YuG.Domain.AI.ValueObjects;

namespace YuG.Domain.AI.Entities;

/// <summary>聊天会话聚合根，包含消息历史和会话元数据。</summary>
public class AiChatSession : AggregateRoot
{
    /// <summary>外部会话标识，供客户端引用。</summary>
    public string SessionId { get; private set; } = string.Empty;

    /// <summary>所属用户标识。</summary>
    public long UserId { get; private set; }

    /// <summary>会话标题。</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>最后活动时间（UTC）。</summary>
    public DateTime LastActiveAt { get; private set; } = DateTime.UtcNow;

    /// <summary>会话消息列表。</summary>
    public List<AiChatMessage> Messages { get; private set; } = [];

    /// <summary>用于 ORM。</summary>
    private AiChatSession()
    {
    }

    /// <summary>创建新聊天会话。</summary>
    /// <param name="userId">用户标识</param>
    /// <param name="systemPrompt">系统提示词，作为首条消息</param>
    public AiChatSession(long userId, string? systemPrompt)
    {
        SessionId = Guid.NewGuid().ToString("N");
        UserId = userId;
        Title = "新对话";
        LastActiveAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            Messages.Add(new AiChatMessage("system", systemPrompt, 0));
        }
    }

    /// <summary>添加一条聊天消息。</summary>
    /// <param name="role">消息角色（system/user/assistant）</param>
    /// <param name="content">消息内容</param>
    /// <param name="tokenCount">Token 数（可选）</param>
    public AiChatMessage AddMessage(string role, string content, int? tokenCount = null)
    {
        var sequence = Messages.Count;
        var message = new AiChatMessage(role, content, sequence, tokenCount);
        Messages.Add(message);
        LastActiveAt = DateTime.UtcNow;
        return message;
    }

    /// <summary>重命名会话。</summary>
    /// <param name="title">新标题</param>
    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("会话标题不能为空");

        if (title.Length > 200)
            throw new DomainException("会话标题长度不能超过 200 个字符");

        Title = title.Trim();
    }

    /// <summary>更新最后活动时间。</summary>
    public void Touch()
    {
        LastActiveAt = DateTime.UtcNow;
    }
}
