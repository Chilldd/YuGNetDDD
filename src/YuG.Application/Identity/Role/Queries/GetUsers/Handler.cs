using Dapper;
using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Interfaces;

namespace YuG.Application.Identity.Role.Queries.GetUsers;

/// <summary>
/// 获取角色关联用户查询处理器
/// </summary>
public class Handler : IRequestHandler<GetRoleUsersQuery, GetRoleUsersResult>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取角色关联用户查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 处理获取角色关联用户查询
    /// </summary>
    /// <param name="request">查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色关联用户结果</returns>
    public async Task<GetRoleUsersResult> Handle(GetRoleUsersQuery request, CancellationToken cancellationToken)
    {
        using var conn = _connectionFactory.CreateConnection();

        // 检查角色是否存在
        var roleExists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Role WHERE Id = @RoleId",
            new { request.RoleId });

        if (roleExists == 0)
            throw new NotFoundException(nameof(Domain.Identity.Entities.Role), request.RoleId);

        var items = await conn.QueryAsync<RoleUserItem>(
            """
            SELECT u.Id, u.Username, u.Status, u.CreatedAt
            FROM User u
            INNER JOIN UserRole ur ON u.Id = ur.UsersId
            WHERE ur.RolesId = @RoleId
            ORDER BY u.Username
            """,
            new { request.RoleId });

        return new GetRoleUsersResult
        {
            Items = items.ToList()
        };
    }
}
