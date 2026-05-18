using Dapper;
using MediatR;
using YuG.Application.Common;
using YuG.Application.Common.Interfaces;
using YuG.Common.Models;

namespace YuG.Application.Permission.Resource.Queries.GetList;

/// <summary>获取资源列表查询处理器。</summary>
public class Handler : IRequestHandler<GetResourceListQuery, PageResult<ResourceListItem>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>初始化处理器。</summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<PageResult<ResourceListItem>> Handle(GetResourceListQuery query, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(query.Type))
        {
            whereClauses.Add("Type = @Type");
            parameters.Add("Type", query.Type);
        }

        if (!string.IsNullOrEmpty(query.HttpMethod))
        {
            whereClauses.Add("HttpMethod = @HttpMethod");
            parameters.Add("HttpMethod", query.HttpMethod.ToUpperInvariant());
        }

        if (query.ParentId.HasValue)
        {
            whereClauses.Add("ParentId = @ParentId");
            parameters.Add("ParentId", query.ParentId.Value);
        }

        if (query.Status.HasValue)
        {
            whereClauses.Add("Status = @Status");
            parameters.Add("Status", query.Status.Value.ToString());
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        return await conn.ToPageResultAsync<ResourceListItem>(
            $"SELECT COUNT(1) FROM Resource {whereSql}",
            $"""
            SELECT Id, Name, Code, Description, Type, HttpMethod, Path,
                   Icon, Route, IsHidden, Badge, PermissionCode,
                   ParentId, SortOrder, Status
            FROM Resource
            {whereSql}
            ORDER BY SortOrder, Id
            LIMIT @PageSize OFFSET @Offset
            """,
            query.Page, query.PageSize,
            parameters);
    }
}
