using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YuG.Domain.Common;
using YuG.Domain.Repositories;
using YuG.Infrastructure.Persistence;

namespace YuG.Infrastructure.Repositories;

/// <summary>
/// 通用仓储基类（为业务层提供统一接口，内部处理ORM实体的操作和映射）
/// </summary>
/// <typeparam name="TAggregate">聚合根类型（Domain 层实体）</typeparam>
/// <typeparam name="TOrmEntity">ORM 实体类型（Infrastructure 层实体）</typeparam>
public abstract class Repository<TAggregate, TOrmEntity> : IRepository<TAggregate> 
    where TAggregate : AggregateRoot
    where TOrmEntity : Infrastructure.Data.Entities.BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TOrmEntity> _dbSet;

    /// <summary>
    /// 初始化仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    protected Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TOrmEntity>();
    }

    /// <summary>
    /// 将 ORM 实体映射到聚合根（子类实现）
    /// </summary>
    /// <param name="ormEntity">ORM 实体</param>
    /// <returns>聚合根</returns>
    protected abstract TAggregate MapToDomain(TOrmEntity ormEntity);

    /// <summary>
    /// 将聚合根映射到 ORM 实体（子类实现）
    /// </summary>
    /// <param name="aggregate">聚合根</param>
    /// <returns>ORM 实体</returns>
    protected abstract TOrmEntity MapToOrmEntity(TAggregate aggregate);

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
        return aggregate;
    }

    /// <inheritdoc />
    public void Update(TAggregate aggregate)
    {
        var ormEntity = MapToOrmEntity(aggregate);
        _dbSet.Update(ormEntity);
    }

    /// <inheritdoc />
    public void Delete(TAggregate aggregate)
    {
        var ormEntity = MapToOrmEntity(aggregate);
        _dbSet.Remove(ormEntity);
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
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

