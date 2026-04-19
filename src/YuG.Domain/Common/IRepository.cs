using System.Linq.Expressions;
using YuG.Domain.Common;

namespace YuG.Domain.Common;

/// <summary>
/// 泛型仓储接口，定义实体的基本持久化操作
/// </summary>
/// <typeparam name="TEntity">聚合根类型</typeparam>
public interface IRepository<TEntity> where TEntity : AggregateRoot
{
    /// <summary>
    /// 根据标识获取实体
    /// </summary>
    /// <param name="id">实体标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>实体实例，不存在则返回 null</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>实体列表</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件查询实体
    /// </summary>
    /// <param name="predicate">查询条件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>满足条件的实体列表</returns>
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">要添加的实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    void Update(TEntity entity);

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    void Delete(TEntity entity);

    /// <summary>
    /// 根据条件判断实体是否存在
    /// </summary>
    /// <param name="predicate">查询条件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在满足条件的实体</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存所有变更
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>受影响的行数</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
