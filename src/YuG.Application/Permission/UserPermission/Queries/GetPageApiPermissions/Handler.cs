using MediatR;
using YuG.Domain.Identity.Enums;
using YuG.Domain.Common.Constants;
using YuG.Domain.Identity.Repositories;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Permission.UserPermission.Queries.GetPageApiPermissions;

/// <summary>
/// 获取页面 API 权限查询处理器
/// </summary>
public class Handler : IRequestHandler<GetPageApiPermissionsQuery, GetPageApiPermissionsResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化获取页面 API 权限查询处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IRoleRepository roleRepository, IResourceRepository resourceRepository)
    {
        _roleRepository = roleRepository;
        _resourceRepository = resourceRepository;
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

        // 超级管理员角色拥有所有资源权限
        var isSuperAdmin = roles.Any(r => r.Code == RoleCodes.SuperAdmin);

        IEnumerable<ResourceEntity> resources;
        if (isSuperAdmin)
        {
            resources = await _resourceRepository.GetActiveAsync(cancellationToken);
        }
        else
        {
            // 收集用户所有角色下的激活资源
            var resourceIds = new HashSet<long>();
            var resourceList = new List<ResourceEntity>();

            foreach (var role in roles.Where(r => r.Status == RoleStatus.Active))
            {
                foreach (var resource in role.Resources.Where(r =>
                    r.Status == ResourceStatus.Active))
                {
                    if (resourceIds.Add(resource.Id))
                    {
                        resourceList.Add(resource);
                    }
                }
            }

            resources = resourceList;
        }

        // 筛选指定页面的 API 权限编码
        var permissionCodes = resources
            .Where(r => r.Type == ResourceType.Api
                && r.ParentId == query.PageId
                && !string.IsNullOrEmpty(r.PermissionCode))
            .Select(r => r.PermissionCode!)
            .Distinct()
            .ToList();

        return new GetPageApiPermissionsResult
        {
            PermissionCodes = permissionCodes
        };
    }
}
