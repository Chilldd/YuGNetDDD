using Dapper;
using MediatR;
using YuG.Application.Common.Interfaces;
using YuG.Domain.Common.Constants;
using YuG.Domain.Permission.Enums;

namespace YuG.Application.Permission.UserPermission.Queries.GetPageApiPermissions;

/// <summary>
/// 获取页面 API 权限查询处理器
/// </summary>
public class Handler : IRequestHandler<GetPageApiPermissionsQuery, GetPageApiPermissionsResult>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取页面 API 权限查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 处理获取页面 API 权限查询
    /// </summary>
    /// <param name="query">获取页面 API 权限查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>页面 API 权限结果</returns>
    public async Task<GetPageApiPermissionsResult> Handle(GetPageApiPermissionsQuery query, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // 查询用户的所有角色
        var roles = (await conn.QueryAsync<RoleInfo>(
            "SELECT Id, Code, Status FROM Role r INNER JOIN UserRole ur ON r.Id = ur.RolesId WHERE ur.UsersId = @UserId",
            new { query.UserId })).ToList();

        // 超级管理员角色拥有所有资源权限
        var isSuperAdmin = roles.Any(r => r.Code == RoleCodes.SuperAdmin);

        IEnumerable<ResourceInfo> resources;
        if (isSuperAdmin)
        {
            resources = await conn.QueryAsync<ResourceInfo>(
                "SELECT Id, Type, ParentId, PermissionCode FROM Resource WHERE Status = 'Active'");
        }
        else
        {
            var activeRoleIds = roles.Where(r => r.Status == "Active").Select(r => r.Id).ToList();

            if (activeRoleIds.Count == 0)
            {
                return new GetPageApiPermissionsResult();
            }

            resources = await conn.QueryAsync<ResourceInfo>(
                """
                SELECT DISTINCT r.Id, r.Type, r.ParentId, r.PermissionCode
                FROM Resource r
                INNER JOIN RoleResource rr ON r.Id = rr.ResourcesId
                WHERE rr.RoleId IN @RoleIds AND r.Status = 'Active'
                """,
                new { RoleIds = activeRoleIds });
        }

        // 筛选指定页面的 API 权限编码
        var permissionCodes = resources
            .Where(r => r.Type == nameof(ResourceType.Api)
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

    private sealed record RoleInfo(long Id, string Code, string Status);

    private sealed record ResourceInfo(long Id, string Type, long? ParentId, string? PermissionCode);
}
