using Microsoft.AspNetCore.Authorization;
using YuG.Application.Common.Interfaces;
using YuG.Domain.Identity.Repositories;
using YuG.Domain.Permission.Enums;

namespace YuG.Api.Authorization;

/// <summary>
/// 权限编码授权处理器（单次请求内缓存权限编码列表）
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserIdentity _userIdentity;
    private readonly IRoleRepository _roleRepository;

    private IReadOnlySet<string>? _cachedPermissions;
    private bool _permissionsLoaded;

    /// <summary>
    /// 初始化权限编码授权处理器
    /// </summary>
    /// <param name="userIdentity">当前用户身份</param>
    /// <param name="roleRepository">角色仓储</param>
    public PermissionAuthorizationHandler(
        IUserIdentity userIdentity,
        IRoleRepository roleRepository)
    {
        _userIdentity = userIdentity;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理权限编码授权要求
    /// </summary>
    /// <param name="context">授权上下文</param>
    /// <param name="requirement">权限编码要求</param>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = await GetUserPermissionsAsync();

        if (permissions.Contains(requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }
    }

    /// <summary>
    /// 获取当前用户的所有权限编码（单次请求内缓存）
    /// </summary>
    private async Task<IReadOnlySet<string>> GetUserPermissionsAsync()
    {
        if (_permissionsLoaded)
        {
            return _cachedPermissions!;
        }

        var roles = await _roleRepository.GetByUserIdWithResourcesAsync(_userIdentity.UserId);

        var permissionCodes = new HashSet<string>();

        foreach (var role in roles.Where(r => r.Status == Domain.Identity.Enums.RoleStatus.Active))
        {
            foreach (var resource in role.Resources.Where(r =>
                r.Status == ResourceStatus.Active
                && r.Type == ResourceType.Api
                && !string.IsNullOrEmpty(r.PermissionCode)))
            {
                permissionCodes.Add(resource.PermissionCode!);
            }
        }

        _cachedPermissions = permissionCodes;
        _permissionsLoaded = true;

        return _cachedPermissions;
    }
}
