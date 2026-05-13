using MediatR;
using YuG.Domain.Identity.Enums;
using YuG.Domain.Identity.Repositories;
using YuG.Domain.Permission.Enums;

namespace YuG.Application.Permission.UserPermission.GetPageApiPermissions;

/// <summary>
/// 获取页面 API 权限查询处理器
/// </summary>
public class Handler : IRequestHandler<GetPageApiPermissionsQuery, GetPageApiPermissionsResult>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化获取页面 API 权限查询处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理获取页面 API 权限查询
    /// </summary>
    /// <param name="query">获取页面 API 权限查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>页面 API 权限结果</returns>
    public async Task<GetPageApiPermissionsResult> Handle(GetPageApiPermissionsQuery query, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetByUserIdWithResourcesAsync(query.UserId, cancellationToken);

        // 收集用户所有角色下的激活资源
        var resourceIds = new HashSet<long>();
        var permissionCodes = new List<string>();

        foreach (var role in roles.Where(r => r.Status == RoleStatus.Active))
        {
            foreach (var resource in role.Resources.Where(r =>
                r.Status == ResourceStatus.Active
                && r.Type == ResourceType.Api
                && r.ParentId == query.PageId
                && !string.IsNullOrEmpty(r.PermissionCode)))
            {
                if (resourceIds.Add(resource.Id))
                {
                    permissionCodes.Add(resource.PermissionCode!);
                }
            }
        }

        return new GetPageApiPermissionsResult
        {
            PermissionCodes = permissionCodes
        };
    }
}
