using Dapper;
using MediatR;
using YuG.Application.Common.Queries;
using YuG.Application.Common.Interfaces;
using YuG.Common.Models;

namespace YuG.Application.Identity.Role.Queries.GetList;

/// <summary>
/// 获取角色列表查询处理器
/// </summary>
public class Handler : IRequestHandler<GetRoleListQuery, PageResult<RoleListItem>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取角色列表查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<PageResult<RoleListItem>> Handle(GetRoleListQuery query, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await conn.ToPageResultAsync<RoleListItem>(
            "SELECT COUNT(1) FROM Role",
            """
            SELECT Id, Name, Code, Description, Status, IsSystem, CreatedAt
            FROM Role
            ORDER BY Id
            LIMIT @PageSize OFFSET @Offset
            """,
            query.Page, query.PageSize);
    }
}
