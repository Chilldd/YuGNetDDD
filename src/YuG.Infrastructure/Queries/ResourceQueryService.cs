using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Application.Queries.Resource;

namespace YuG.Infrastructure.Queries;

/// <summary>
/// 资源查询服务实现（使用 Dapper）
/// </summary>
public class ResourceQueryService : IResourceQueryService
{
    private readonly string _connectionString;

    /// <summary>
    /// 初始化资源查询服务
    /// </summary>
    /// <param name="configuration">配置</param>
    public ResourceQueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
    }

    /// <summary>
    /// 获取所有资源
    /// </summary>
    /// <param name="httpMethod">HTTP 方法筛选（可选）</param>
    /// <param name="parentId">父级资源标识筛选（可选）</param>
    /// <param name="activeOnly">是否只返回激活状态（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表响应</returns>
    public async Task<ResourceListResponse> GetAllAsync(
        string? httpMethod = null,
        Guid? parentId = null,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM Resources WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(httpMethod))
        {
            sql += " AND HttpMethod = @HttpMethod";
            parameters.Add("HttpMethod", httpMethod);
        }

        if (parentId.HasValue)
        {
            sql += " AND ParentId = @ParentId";
            parameters.Add("ParentId", parentId.Value);
        }

        if (activeOnly == true)
        {
            sql += " AND Status = 'Active'";
        }

        sql += " ORDER BY SortOrder";

        var entities = await connection.QueryAsync<ResourceEntityDto>(sql, parameters);
        var responses = entities.Select(MapToResponse).ToList();

        return new ResourceListResponse
        {
            Resources = responses,
            TotalCount = responses.Count
        };
    }

    /// <summary>
    /// 根据标识获取资源
    /// </summary>
    /// <param name="id">资源标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "SELECT * FROM Resources WHERE Id = @Id";
        var entity = await connection.QueryFirstOrDefaultAsync<ResourceEntityDto>(sql, new { Id = id });

        return entity is null ? null : MapToResponse(entity);
    }

    private static ResourceResponse MapToResponse(ResourceEntityDto entity)
    {
        return new ResourceResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description,
            HttpMethod = entity.HttpMethod,
            Path = entity.Path,
            ParentId = entity.ParentId,
            SortOrder = entity.SortOrder,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private class ResourceEntityDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string HttpMethod { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public Guid? ParentId { get; init; }
        public int SortOrder { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
