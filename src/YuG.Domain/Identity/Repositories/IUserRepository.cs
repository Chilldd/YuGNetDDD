using YuG.Common.Models;
using YuG.Domain.Common;
using YuG.Domain.Identity.Entities;

namespace YuG.Domain.Identity.Repositories;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户实体，不存在则返回 null</returns>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户名是否存在</returns>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标识获取用户（包含角色导航）
    /// </summary>
    /// <param name="id">用户标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户实体，不存在则返回 null</returns>
    Task<User?> GetByIdWithRolesAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标识获取用户（包含刷新令牌导航）
    /// </summary>
    /// <param name="id">用户标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户实体，不存在则返回 null</returns>
    Task<User?> GetByIdWithRefreshTokensAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页获取用户列表。
    /// </summary>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户分页结果</returns>
    Task<PageResult<User>> GetUsersPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
