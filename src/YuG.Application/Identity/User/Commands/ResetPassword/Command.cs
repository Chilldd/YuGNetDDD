using FluentValidation;
using YuG.Application.Common;
using UserResult = YuG.Application.Identity.User.Commands.Create.UserResult;

namespace YuG.Application.Identity.User.Commands.ResetPassword;

/// <summary>
/// 重置用户密码命令
/// </summary>
public class ResetPasswordCommand : CommandBase<UserResult>
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 重置用户密码命令验证器
/// </summary>
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>
    /// 初始化重置用户密码命令验证器
    /// </summary>
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户标识必须大于 0");
    }
}
