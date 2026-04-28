using YuG.Domain.Common;
using YuG.Domain.Permission.Entities;
using YuG.Domain.Permission.Enums;

namespace YuG.Domain.Permission.Repositories;

/// <summary>
/// 资源仓储接口
/// </summary>
public interface IResourceRepository : IRepository<Resource>
{
    /// <summary>
    /// 根据资源编码获取资源
    /// </summary>
    /// <param name="code">资源编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源实体，不存在则返回 null</returns>
    Task<Resource?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据资源类型获取资源列表
    /// </summary>
    /// <param name="type">资源类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表</returns>
    Task<IReadOnlyList<Resource>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据权限编码获取按钮资源
    /// </summary>
    /// <param name="permissionCode">权限编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源实体，不存在则返回 null</returns>
    Task<Resource?> GetByPermissionCodeAsync(string permissionCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查资源编码是否存在
    /// </summary>
    /// <param name="code">资源编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源编码是否存在</returns>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 HTTP 方法获取资源列表
    /// </summary>
    /// <param name="httpMethod">HTTP 方法</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表</returns>
    Task<IReadOnlyList<Resource>> GetByHttpMethodAsync(ResourceHttpMethod httpMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 HTTP 方法获取资源列表
    /// </summary>
    /// <param name="httpMethod">HTTP 方法名称（GET/POST/PUT/DELETE）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表</returns>
    Task<IReadOnlyList<Resource>> GetByHttpMethodAsync(string httpMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据父级资源标识获取子资源列表
    /// </summary>
    /// <param name="parentId">父级资源标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>子资源列表</returns>
    Task<IReadOnlyList<Resource>> GetByParentIdAsync(long parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有激活状态的资源
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>激活的资源列表</returns>
    Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标识列表获取资源
    /// </summary>
    /// <param name="ids">资源标识列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表</returns>
    Task<IReadOnlyList<Resource>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
}
