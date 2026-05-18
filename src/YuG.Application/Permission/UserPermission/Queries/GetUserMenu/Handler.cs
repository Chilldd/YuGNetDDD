using Dapper;
using MediatR;
using YuG.Application.Common.Interfaces;
using YuG.Domain.Common.Constants;
using YuG.Domain.Permission.Enums;

namespace YuG.Application.Permission.UserPermission.Queries.GetUserMenu;

/// <summary>
/// 获取当前用户菜单查询处理器
/// </summary>
public class Handler : IRequestHandler<GetUserMenuQuery, GetUserMenuResult>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取当前用户菜单查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 处理获取当前用户菜单查询
    /// </summary>
    /// <param name="query">获取当前用户菜单查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户菜单结果</returns>
    public async Task<GetUserMenuResult> Handle(GetUserMenuQuery query, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // 查询用户的所有角色
        var roles = (await conn.QueryAsync<RoleInfo>(
            "SELECT Id, Code, Status FROM Role r INNER JOIN UserRole ur ON r.Id = ur.RolesId WHERE ur.UsersId = @UserId",
            new { query.UserId })).ToList();

        // 超级管理员角色拥有所有资源权限
        var isSuperAdmin = roles.Any(r => r.Code == RoleCodes.SuperAdmin);

        IReadOnlyCollection<MenuResourceInfo> resources;
        if (isSuperAdmin)
        {
            resources = (await conn.QueryAsync<MenuResourceInfo>(
                """
                SELECT Id, Name, Code, Icon, Route, IsHidden, Badge, SortOrder, PermissionCode, Type, ParentId
                FROM Resource
                WHERE Status = 'Active' AND (Type = 'Menu' OR Type = 'Page')
                """)).ToList();
        }
        else
        {
            var activeRoleIds = roles.Where(r => r.Status == "Active").Select(r => r.Id).ToList();

            if (activeRoleIds.Count == 0)
            {
                return new GetUserMenuResult();
            }

            resources = (await conn.QueryAsync<MenuResourceInfo>(
                """
                SELECT DISTINCT r.Id, r.Name, r.Code, r.Icon, r.Route, r.IsHidden, r.Badge,
                       r.SortOrder, r.PermissionCode, r.Type, r.ParentId
                FROM Resource r
                INNER JOIN RoleResource rr ON r.Id = rr.ResourcesId
                WHERE rr.RoleId IN @RoleIds AND r.Status = 'Active' AND (r.Type = 'Menu' OR r.Type = 'Page')
                """,
                new { RoleIds = activeRoleIds })).ToList();
        }

        // 筛选 Menu 和 Page 类型的资源
        var menuResources = resources.Where(r => r.Type == nameof(ResourceType.Menu)).ToList();
        var pageResources = resources.Where(r => r.Type == nameof(ResourceType.Page)).ToList();

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
    private static List<UserMenuTreeItem> BuildPageChildren(long parentId, List<MenuResourceInfo> pages)
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

    private sealed record RoleInfo(long Id, string Code, string Status);

    private sealed record MenuResourceInfo(
        long Id, string Name, string Code, string? Icon, string? Route,
        bool IsHidden, string? Badge, int SortOrder, string? PermissionCode,
        string Type, long? ParentId);
}
