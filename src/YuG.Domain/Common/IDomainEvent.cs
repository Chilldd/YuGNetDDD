using MediatR;

namespace YuG.Domain.Common;

/// <summary>
/// 领域事件接口（继承 MediatR INotification 支持直接发布）
/// </summary>
public interface IDomainEvent : INotification
{
}
