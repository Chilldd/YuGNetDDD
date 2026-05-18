using Microsoft.EntityFrameworkCore;
using YuG.Domain.Common;
using YuG.Domain.Permission.Entities;
using YuG.Domain.Permission.Repositories;

namespace YuG.Infrastructure.Persistence.Permission.Repositories;

/// <summary>
/// 资源仓储实现
/// </summary>
public class ResourceRepository : Repository<Resource>, IResourceRepository
{
    /// <summary>
    /// 初始化资源仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="domainEventPublisher">领域事件发布器</param>
    public ResourceRepository(ApplicationDbContext context, IDomainEventPublisher domainEventPublisher)
        : base(context, domainEventPublisher)
    {
    }

    /// <summary>
    /// 根据资源编码获取资源
    /// </summary>
    /// <param name="code">资源编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源实体，不存在则返回 null</returns>
    public async Task<Resource?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    /// <summary>
    /// 检查资源编码是否存在
    /// </summary>
    /// <param name="code">资源编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源编码是否存在</returns>
    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .AnyAsync(r => r.Code == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Resource>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await _context.Resources
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }
}
