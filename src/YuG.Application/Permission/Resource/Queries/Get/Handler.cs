using Dapper;
using MediatR;
using YuG.Application.Common.Interfaces;

namespace YuG.Application.Permission.Resource.Queries.Get;

/// <summary>
/// 获取资源查询处理器
/// </summary>
public class Handler : IRequestHandler<GetResourceQuery, GetResourceResult?>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取资源查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 处理获取资源查询
    /// </summary>
    /// <param name="query">获取资源查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源结果</returns>
    public async Task<GetResourceResult?> Handle(GetResourceQuery query, CancellationToken cancellationToken)
    {
        using var conn = _connectionFactory.CreateConnection();

        return await conn.QueryFirstOrDefaultAsync<GetResourceResult>(
            """
            SELECT Id, Name, Code, Description, Type, HttpMethod, Path,
                   Icon, Route, IsHidden, Badge, PermissionCode,
                   ParentId, SortOrder, Status, CreatedAt, UpdatedAt
            FROM Resource
            WHERE Id = @Id
            """,
            new { query.Id });
    }
}
