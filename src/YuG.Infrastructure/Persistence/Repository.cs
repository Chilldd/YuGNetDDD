using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YuG.Domain.Common;

namespace YuG.Infrastructure.Persistence;

/// <summary>
/// 通用仓储基类（直接使用 Domain 实体，无需映射）
/// </summary>
/// <typeparam name="TAggregate">聚合根类型（Domain 层实体）</typeparam>
public abstract class Repository<TAggregate> : IRepository<TAggregate>
    where TAggregate : AggregateRoot
{
    protected readonly ApplicationDbContext _context;
    private readonly IDomainEventPublisher _domainEventPublisher;
    protected readonly DbSet<TAggregate> _dbSet;
    /// <summary>
    /// 初始化仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="domainEventPublisher">领域事件发布器</param>
    protected Repository(
        ApplicationDbContext context,
        IDomainEventPublisher domainEventPublisher)
    {
        _context = context;
        _domainEventPublisher = domainEventPublisher;
        _dbSet = context.Set<TAggregate>();
    }

    /// <inheritdoc />
    public async Task<TAggregate?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public IQueryable<TAggregate> GetQueryable()
    {
        return _dbSet;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TAggregate>> FindAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TAggregate> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(aggregate, cancellationToken);
        return aggregate;
    }

    /// <inheritdoc />
    public void Update(TAggregate aggregate)
    {
        _dbSet.Update(aggregate);
    }

    /// <inheritdoc />
    public void Delete(TAggregate aggregate)
    {
        _dbSet.Remove(aggregate);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 获取当前被 EF Core 追踪且有领域事件的聚合根
        var aggregatesWithEvents = _context.ChangeTracker.Entries<TAggregate>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        // 保存变更到数据库
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 收集并发布领域事件
        var domainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        if (domainEvents.Count > 0)
        {
            await _domainEventPublisher.PublishAsync(domainEvents, cancellationToken);
        }

        // 清空所有领域事件
        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
