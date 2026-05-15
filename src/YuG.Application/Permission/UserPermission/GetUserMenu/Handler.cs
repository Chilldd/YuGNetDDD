using MediatR;
using YuG.Domain.Identity.Enums;
using YuG.Domain.Common.Constants;
using YuG.Domain.Identity.Repositories;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Permission.UserPermission.GetUserMenu;

/// <summary>
/// 获取当前用户菜单查询处理器
/// </summary>
public class Handler : IRequestHandler<GetUserMenuQuery, GetUserMenuResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化获取当前用户菜单查询处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IRoleRepository roleRepository, IResourceRepository resourceRepository)
    {
        _roleRepository = roleRepository;
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理获取当前用户菜单查询
    /// </summary>
    /// <param name="query">获取当前用户菜单查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户菜单结果</returns>
    public async Task<GetUserMenuResult> Handle(GetUserMenuQuery query, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetByUserIdWithResourcesAsync(query.UserId, cancellationToken);

        // 超级管理员角色拥有所有资源权限
        var isSuperAdmin = roles.Any(r => r.Code == RoleCodes.SuperAdmin);

        IReadOnlyCollection<ResourceEntity> resources;
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
                foreach (var resource in role.Resources.Where(r => r.Status == ResourceStatus.Active))
                {
                    if (resourceIds.Add(resource.Id))
                    {
                        resourceList.Add(resource);
                    }
                }
            }

            resources = resourceList;
        }

        // 筛选 Menu 和 Page 类型的资源
        var menuResources = resources
            .Where(r => r.Type == ResourceType.Menu)
            .ToList();

        var pageResources = resources
            .Where(r => r.Type == ResourceType.Page)
            .ToList();

        // 构建 Menu -> Page 树
        var menuItems = menuResources.Select(m => new UserMenuTreeItem
        {
            Id = m.Id,
            Name = m.Name,
            Code = m.Code,
            Icon = m.Icon,
            Route = m.Route,
            IsHidden = m.IsHidden,
            Badge = m.Badge,
            SortOrder = m.SortOrder,
            Children = BuildPageChildren(m.Id, pageResources)
        })
        .OrderBy(x => x.SortOrder)
        .ToList();

        // 收集已被 Menu 引用的 Page ID
        var attachedPageIds = menuResources
            .SelectMany(m => pageResources.Where(p => p.ParentId == m.Id))
            .Select(p => p.Id)
            .ToHashSet();

        // 无父级 Page 作为顶层节点（首页等顶级页面）
        var orphanPages = pageResources
            .Where(p => p.ParentId is null && !attachedPageIds.Contains(p.Id))
            .OrderBy(p => p.SortOrder)
            .Select(p => new UserMenuTreeItem
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Icon = p.Icon,
                Route = p.Route,
                IsHidden = p.IsHidden,
                Badge = p.Badge,
                SortOrder = p.SortOrder,
                PermissionCode = p.PermissionCode
            })
            .ToList();

        var allItems = menuItems.Concat(orphanPages)
            .OrderBy(x => x.SortOrder)
            .ToList();

        return new GetUserMenuResult
        {
            Items = allItems
        };
    }

    /// <summary>
    /// 构建菜单子页面列表
    /// </summary>
    private static List<UserMenuTreeItem> BuildPageChildren(long parentId, List<ResourceEntity> pages)
    {
        return pages
            .Where(p => p.ParentId == parentId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new UserMenuTreeItem
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Route = p.Route,
                PermissionCode = p.PermissionCode,
                SortOrder = p.SortOrder
            })
            .ToList();
    }
}
