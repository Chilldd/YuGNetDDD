using Microsoft.EntityFrameworkCore;
using YuG.Domain.Permission.Entities;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using YuG.Domain.Common;
using YuG.Infrastructure.Persistence;

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

    /// <summary>
    /// 根据 HTTP 方法获取资源列表
    /// </summary>
    /// <param name="httpMethod">HTTP 方法</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表</returns>
    public async Task<IReadOnlyList<Resource>> GetByHttpMethodAsync(ResourceHttpMethod httpMethod, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .Where(r => r.HttpMethod == httpMethod)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据 HTTP 方法获取资源列表
    /// </summary>
    /// <param name="httpMethod">HTTP 方法名称（GET/POST/PUT/DELETE）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表</returns>
    public async Task<IReadOnlyList<Resource>> GetByHttpMethodAsync(string httpMethod, CancellationToken cancellationToken = default)
    {
        var parsed = Enum.Parse<ResourceHttpMethod>(httpMethod, ignoreCase: true);
        return await GetByHttpMethodAsync(parsed, cancellationToken);
    }

    /// <summary>
    /// 根据父级资源标识获取子资源列表
    /// </summary>
    /// <param name="parentId">父级资源标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>子资源列表</returns>
    public async Task<IReadOnlyList<Resource>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .Where(r => r.ParentId == parentId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取所有激活状态的资源
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>激活的资源列表</returns>
    public async Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .AsNoTracking()
            .Where(r => r.Status == ResourceStatus.Active)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
