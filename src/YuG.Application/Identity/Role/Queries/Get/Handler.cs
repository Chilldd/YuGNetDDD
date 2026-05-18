using Dapper;
using MediatR;
using YuG.Application.Common.Interfaces;

namespace YuG.Application.Identity.Role.Queries.Get;

/// <summary>
/// 获取单个角色查询处理器
/// </summary>
public class Handler : IRequestHandler<GetRoleQuery, GetRoleResult?>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取单个角色查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 处理获取单个角色查询
    /// </summary>
    /// <param name="query">获取单个角色查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色查询结果，不存在则返回 null</returns>
    public async Task<GetRoleResult?> Handle(GetRoleQuery query, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var role = await conn.QueryFirstOrDefaultAsync<GetRoleResult>(
            """
            SELECT Id, Name, Code, Description, Status, IsSystem, CreatedAt, UpdatedAt
            FROM Role
            WHERE Id = @Id AND IsSystem = 0
            """,
            new { query.Id });

        if (role is null)
            return null;

        var resourceIds = (await conn.QueryAsync<long>(
            "SELECT ResourcesId FROM RoleResource WHERE RoleId = @Id",
            new { query.Id })).ToList();

        return role with { ResourceIds = resourceIds };
    }
}
