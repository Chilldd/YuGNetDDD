using YuG.Domain.Common;

namespace YuG.Domain.Identity.Events;

/// <summary>
/// 用户角色变更领域事件
/// </summary>
/// <param name="UserId">用户标识</param>
/// <param name="NewRoleIds">新分配的角色 ID 集合</param>
public sealed record UserRolesChangedEvent(
    long UserId,
    IReadOnlyList<long> NewRoleIds
) : IDomainEvent;
