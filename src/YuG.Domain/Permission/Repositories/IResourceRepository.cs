using YuG.Domain.Common;
using YuG.Domain.Permission.Entities;

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
    /// 检查资源编码是否存在
    /// </summary>
    /// <param name="code">资源编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源编码是否存在</returns>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标识列表获取资源
    /// </summary>
    /// <param name="ids">资源标识列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表</returns>
    Task<IReadOnlyList<Resource>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
}
