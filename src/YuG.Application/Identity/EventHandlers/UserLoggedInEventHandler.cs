using MediatR;
using YuG.Application.Common;
using YuG.Domain.Common.Interfaces;
using YuG.Domain.Identity.Events;

namespace YuG.Application.Identity.EventHandlers;

/// <summary>
/// 用户登录成功事件处理器 — 更新缓存中的令牌世代版本
/// </summary>
public class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly ICache _cache;

    /// <summary>
    /// 初始化用户登录成功事件处理器
    /// </summary>
    /// <param name="cache">缓存服务</param>
    public UserLoggedInEventHandler(ICache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// 处理用户登录成功事件
    /// </summary>
    /// <param name="notification">事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        await _cache.SetAsync(
            CacheKeys.TokenGeneration(notification.UserId),
            notification.NewGeneration.ToString(),
            TimeSpan.FromDays(7),
            cancellationToken);
    }
}
