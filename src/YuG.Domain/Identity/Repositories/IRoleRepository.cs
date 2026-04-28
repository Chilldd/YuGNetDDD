using YuG.Domain.Common;
using YuG.Domain.Identity.Entities;

namespace YuG.Domain.Identity.Repositories;

/// <summary>
/// 角色仓储接口
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// 根据角色编码获取角色
    /// </summary>
    /// <param name="code">角色编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色实体，不存在则返回 null</returns>
    Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查角色编码是否存在
    /// </summary>
    /// <param name="code">角色编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色编码是否存在</returns>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定用户的所有角色
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色列表</returns>
    Task<IReadOnlyList<Role>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定 ID 列表的角色
    /// </summary>
    /// <param name="ids">角色标识列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色列表</returns>
    Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标识获取角色（包含资源导航）
    /// </summary>
    /// <param name="id">角色标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色实体，不存在则返回 null</returns>
    Task<Role?> GetByIdWithResourcesAsync(long id, CancellationToken cancellationToken = default);
}
