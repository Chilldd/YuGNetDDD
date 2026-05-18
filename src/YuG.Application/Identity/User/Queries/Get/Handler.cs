using Dapper;
using MediatR;
using YuG.Application.Common.Interfaces;

namespace YuG.Application.Identity.User.Queries.Get;

/// <summary>
/// 获取单个用户查询处理器
/// </summary>
public class Handler : IRequestHandler<GetUserQuery, GetUserResult?>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取单个用户查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 处理获取单个用户查询
    /// </summary>
    /// <param name="query">获取单个用户查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户查询结果，不存在则返回 null</returns>
    public async Task<GetUserResult?> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var user = await conn.QueryFirstOrDefaultAsync<GetUserResult>(
            "SELECT Id, Username, Status, CreatedAt, UpdatedAt FROM User WHERE Id = @Id",
            new { query.Id });

        if (user is null)
            return null;

        var roleIds = (await conn.QueryAsync<long>(
            "SELECT RolesId FROM UserRole WHERE UsersId = @Id",
            new { query.Id })).ToList();

        return user with { RoleIds = roleIds };
    }
}
