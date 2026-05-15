using YuG.Domain.Common;

namespace YuG.Domain.Identity.Events;

/// <summary>
/// 用户密码重置领域事件
/// </summary>
/// <param name="UserId">用户标识</param>
/// <param name="Username">用户名</param>
/// <param name="NewGeneration">新的令牌世代版本</param>
public sealed record UserPasswordResetEvent(
    long UserId,
    string Username,
    int NewGeneration
) : IDomainEvent;
