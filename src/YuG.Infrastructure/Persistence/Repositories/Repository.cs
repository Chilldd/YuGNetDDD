using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YuG.Domain.Common;
using YuG.Domain.Repositories;
using YuG.Infrastructure.Persistence;
using YuG.Infrastructure.Persistence.Entities;

namespace YuG.Infrastructure.Persistence.Repositories;

/// <summary>
/// 通用仓储基类（为业务层提供统一接口，内部处理ORM实体的操作和映射）
/// </summary>
/// <typeparam name="TAggregate">聚合根类型（Domain 层实体）</typeparam>
/// <typeparam name="TOrmEntity">ORM 实体类型（Infrastructure 层实体）</typeparam>
public abstract class Repository<TAggregate, TOrmEntity> : IRepository<TAggregate>
    where TAggregate : AggregateRoot
    where TOrmEntity : Entities.BaseEntity
{
    protected readonly ApplicationDbContext _context;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly DbSet<TOrmEntity> _dbSet;
    private readonly HashSet<TAggregate> _trackedAggregates = [];
    private readonly Func<TOrmEntity, TAggregate> _mapToDomain;
    private readonly Func<TAggregate, TOrmEntity> _mapToOrmEntity;

    /// <summary>
    /// 初始化仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="domainEventPublisher">领域事件发布器</param>
    /// <param name="mapToDomain">ORM 实体到聚合根的映射函数</param>
    /// <param name="mapToOrmEntity">聚合根到 ORM 实体的映射函数</param>
    protected Repository(
        ApplicationDbContext context,
        IDomainEventPublisher domainEventPublisher,
        Func<TOrmEntity, TAggregate> mapToDomain,
        Func<TAggregate, TOrmEntity> mapToOrmEntity)
    {
        _context = context;
        _domainEventPublisher = domainEventPublisher;
        _mapToDomain = mapToDomain;
        _mapToOrmEntity = mapToOrmEntity;
        _dbSet = context.Set<TOrmEntity>();
    }

    /// <summary>
    /// 将 ORM 实体映射到聚合根
    /// </summary>
    /// <param name="ormEntity">ORM 实体</param>
    /// <returns>聚合根</returns>
    protected TAggregate MapToDomain(TOrmEntity ormEntity) => _mapToDomain(ormEntity);

    /// <summary>
    /// 将聚合根映射到 ORM 实体
    /// </summary>
    /// <param name="aggregate">聚合根</param>
    /// <returns>ORM 实体</returns>
    protected TOrmEntity MapToOrmEntity(TAggregate aggregate) => _mapToOrmEntity(aggregate);

    /// <inheritdoc />
    public async Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ormEntity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return ormEntity == null ? null : MapToDomain(ormEntity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var ormEntities = await _dbSet.ToListAsync(cancellationToken);
        return ormEntities.ConvertAll(MapToDomain);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TAggregate>> FindAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // 注意：这是简化实现，实际应用中需要处理表达式树转换
        var allEntities = await GetAllAsync(cancellationToken);
        return allEntities.Where(predicate.Compile()).ToList();
    }

    /// <inheritdoc />
    public async Task<TAggregate> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var ormEntity = MapToOrmEntity(aggregate);
        await _dbSet.AddAsync(ormEntity, cancellationToken);
        _trackedAggregates.Add(aggregate);
        return aggregate;
    }

    /// <inheritdoc />
    public void Update(TAggregate aggregate)
    {
        var ormEntity = MapToOrmEntity(aggregate);
        _dbSet.Update(ormEntity);
        _trackedAggregates.Add(aggregate);
    }

    /// <inheritdoc />
    public void Delete(TAggregate aggregate)
    {
        var ormEntity = MapToOrmEntity(aggregate);
        _dbSet.Remove(ormEntity);
        _trackedAggregates.Add(aggregate);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var allEntities = await GetAllAsync(cancellationToken);
        return allEntities.Any(predicate.Compile());
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 保存变更到数据库
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 收集所有已操作聚合根中的领域事件
        var domainEvents = _trackedAggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // 发布领域事件
        if (domainEvents.Any())
        {
            await _domainEventPublisher.PublishAsync(domainEvents, cancellationToken);
        }

        // 清空所有领域事件和跟踪集合
        foreach (var aggregate in _trackedAggregates)
        {
            aggregate.ClearDomainEvents();
        }
        _trackedAggregates.Clear();

        return result;
    }
}
