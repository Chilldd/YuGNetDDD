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
}
