using Microsoft.EntityFrameworkCore;
using YuG.Domain.Common;
using YuG.Domain.Identity.Entities;
using YuG.Domain.Identity.Repositories;
using YuG.Infrastructure.Persistence;

namespace YuG.Infrastructure.Persistence.Identity.Repositories;

/// <summary>
/// 角色仓储实现
/// </summary>
public class RoleRepository : Repository<Role>, IRoleRepository
{
    /// <summary>
    /// 初始化角色仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="domainEventPublisher">领域事件发布器</param>
    public RoleRepository(ApplicationDbContext context, IDomainEventPublisher domainEventPublisher)
        : base(context, domainEventPublisher)
    {
    }

    /// <inheritdoc />
    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .AsNoTracking()
            .AnyAsync(r => r.Code == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Role>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .AsNoTracking()
            .Where(r => r.Users.Any(u => u.Id == userId))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await _context.Set<Role>()
            .AsNoTracking()
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Role?> GetByIdWithResourcesAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.Resources)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
