namespace YuG.Application.AI.Session.GetList;

/// <summary>会话列表结果。</summary>
public record GetSessionListResult
{
    /// <summary>会话列表项。</summary>
    public IReadOnlyList<SessionListItem> Items { get; init; } = [];
}

/// <summary>会话列表项。</summary>
public record SessionListItem
{
    /// <summary>会话 ID。</summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>会话标题。</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>最后活动时间（UTC）。</summary>
    public DateTime LastActiveAt { get; init; }
}
