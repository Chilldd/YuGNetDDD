using Dapper;
using MediatR;
using YuG.Application.Common.Queries;
using YuG.Application.Common.Interfaces;
using YuG.Common.Models;

namespace YuG.Application.Identity.User.Queries.GetList;

/// <summary>
/// 获取用户列表查询处理器
/// </summary>
public class Handler : IRequestHandler<GetUserListQuery, PageResult<UserListItem>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取用户列表查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<PageResult<UserListItem>> Handle(GetUserListQuery query, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await conn.ToPageResultAsync<UserListItem>(
            "SELECT COUNT(1) FROM User",
            """
            SELECT Id, Username, Status, CreatedAt
            FROM User
            ORDER BY Id
            LIMIT @PageSize OFFSET @Offset
            """,
            query.Page, query.PageSize);
    }
}
