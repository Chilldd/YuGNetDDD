using YuG.Domain.Common;

namespace YuG.Domain.Common;

/// <summary>
/// 领域事件发布器接口
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// 发布单个领域事件
    /// </summary>
    /// <param name="domainEvent">领域事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布多个领域事件
    /// </summary>
    /// <param name="domainEvents">领域事件集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
