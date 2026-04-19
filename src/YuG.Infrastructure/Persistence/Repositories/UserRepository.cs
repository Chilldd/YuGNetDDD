using Microsoft.EntityFrameworkCore;
using YuG.Domain.Common;
using YuG.Domain.Identity.Entities;
using YuG.Domain.Identity.Repositories;
using YuG.Infrastructure.Persistence.Mappings;
using YuG.Infrastructure.Persistence.Entities.Auth;

namespace YuG.Infrastructure.Persistence.Repositories;

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : Repository<User, UserEntity>, IUserRepository
{
    /// <summary>
    /// 初始化用户仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="domainEventPublisher">领域事件发布器</param>
    public UserRepository(ApplicationDbContext context, IDomainEventPublisher domainEventPublisher)
        : base(context, domainEventPublisher, UserMapping.ToDomain, UserMapping.ToEntity)
    {
    }

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户实体，不存在则返回 null</returns>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        return userEntity?.ToDomain();
    }

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户名是否存在</returns>
    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username, cancellationToken);
    }
}

