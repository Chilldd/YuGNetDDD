using YuG.Domain.Common;

namespace YuG.Domain.Identity.Events;

/// <summary>
/// 角色资源分配变更领域事件
/// </summary>
/// <param name="RoleId">角色标识</param>
/// <param name="RoleCode">角色编码</param>
/// <param name="ResourceIds">分配的资源 ID 集合</param>
public sealed record RoleResourcesUpdatedEvent(
    long RoleId,
    string RoleCode,
    IReadOnlyList<long> ResourceIds
) : IDomainEvent;
