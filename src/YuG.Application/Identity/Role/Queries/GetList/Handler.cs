using Dapper;
using MediatR;
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
        using var conn = _connectionFactory.CreateConnection();

        var totalCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Role");

        var offset = (query.Page - 1) * query.PageSize;
        var items = await conn.QueryAsync<RoleListItem>(
            """
            SELECT Id, Name, Code, Description, Status, IsSystem, CreatedAt
            FROM Role
            ORDER BY Id
            LIMIT @PageSize OFFSET @Offset
            """,
            new { query.PageSize, Offset = offset });

        return new PageResult<RoleListItem>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }
}
