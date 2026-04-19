using YuG.Application.DTOs.Resource.Responses;

namespace YuG.Application.Domains.Resource.Queries;

/// <summary>
/// 资源查询服务接口
/// </summary>
public interface IResourceQueryService
{
    /// <summary>
    /// 获取所有资源
    /// </summary>
    /// <param name="httpMethod">HTTP 方法筛选（可选）</param>
    /// <param name="parentId">父级资源标识筛选（可选）</param>
    /// <param name="activeOnly">是否只返回激活状态（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表响应</returns>
    Task<ResourceListResponse> GetAllAsync(
        string? httpMethod = null,
        Guid? parentId = null,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标识获取资源
    /// </summary>
    /// <param name="id">资源标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    Task<ResourceResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
