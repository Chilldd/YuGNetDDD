using MediatR;
using YuG.Domain.Common;

namespace YuG.Infrastructure.DomainEvents;

/// <summary>
/// 基于 MediatR 的领域事件发布器
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化领域事件发布器
    /// </summary>
    /// <param name="mediator">MediatR 中介器</param>
    public DomainEventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _mediator.Publish(domainEvent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
