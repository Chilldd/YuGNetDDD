namespace YuG.Domain.Common;

/// <summary>
/// 聚合根基类，提供唯一标识和领域事件支持（纯业务，无基础设施关注点）
/// </summary>
public abstract class AggregateRoot
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// 领域事件集合（只读）
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 添加领域事件
    /// </summary>
    /// <param name="domainEvent">领域事件</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// 移除领域事件
    /// </summary>
    /// <param name="domainEvent">领域事件</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// 清空所有领域事件
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// 重写相等性比较，基于 Id 属性判断
    /// </summary>
    /// <param name="obj">比较的对象</param>
    /// <returns>是否相等</returns>
    public override bool Equals(object? obj)
    {
        return obj is AggregateRoot entity && Id == entity.Id;
    }

    /// <summary>
    /// 获取哈希值，基于 Id 属性
    /// </summary>
    /// <returns>哈希值</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
