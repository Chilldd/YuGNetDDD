using YuG.Domain.Common;

namespace YuG.Domain.AI.Events;

/// <summary>会话首次对话完成，请求生成标题。</summary>
/// <param name="SessionId">会话 ID</param>
/// <param name="UserId">用户标识</param>
public sealed record SessionTitleGenerationRequested(
    string SessionId,
    long UserId
) : IDomainEvent;
