using FluentValidation;
using YuG.Application.Common;
using YuG.Application.Identity.User.DTOs;

namespace YuG.Application.Identity.User.Commands.Disable;

/// <summary>
/// 禁用用户命令
/// </summary>
public class DisableUserCommand : CommandBase<UserResult>
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 禁用用户命令验证器
/// </summary>
public class DisableUserCommandValidator : AbstractValidator<DisableUserCommand>
{
    /// <summary>
    /// 初始化禁用用户命令验证器
    /// </summary>
    public DisableUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户标识必须大于 0");
    }
}
