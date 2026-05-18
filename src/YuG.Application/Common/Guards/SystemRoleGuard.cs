using YuG.Domain.Common;
using YuG.Domain.Identity.Entities;

namespace YuG.Application.Common.Guards;

/// <summary>
/// 系统内置角色守卫，禁止对系统内置角色进行任何修改操作
/// </summary>
public static class SystemRoleGuard
{
    /// <summary>
    /// 检查角色是否为系统内置角色，是则抛出异常
    /// </summary>
    /// <param name="role">要检查的角色</param>
    /// <exception cref="DomainException">系统内置角色不允许修改</exception>
    public static void AgainstModification(Role role)
    {
        if (role.IsSystem)
        {
            throw new DomainException("系统内置角色不允许修改");
        }
    }

    /// <summary>
    /// 检查角色列表中是否包含系统内置角色，包含则抛出异常
    /// </summary>
    /// <param name="roles">角色列表</param>
    /// <exception cref="DomainException">系统内置角色不允许通过接口分配</exception>
    public static void AgainstAssigningSystemRoles(IEnumerable<Role> roles)
    {
        var systemRoles = roles.Where(r => r.IsSystem).ToList();
        if (systemRoles.Count == 0)
        {
            return;
        }

        var names = string.Join(", ", systemRoles.Select(r => $"'{r.Name}'"));
        throw new DomainException($"系统内置角色不允许通过接口分配：{names}");
    }
}
