using FluentValidation;
using YuG.Application.Common;
using YuG.Application.Identity.User.DTOs;

namespace YuG.Application.Identity.User.Commands.Activate;

/// <summary>
/// 启用用户命令
/// </summary>
public class ActivateUserCommand : CommandBase<UserResult>
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 启用用户命令验证器
/// </summary>
public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    /// <summary>
    /// 初始化启用用户命令验证器
    /// </summary>
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户标识必须大于 0");
    }
}
