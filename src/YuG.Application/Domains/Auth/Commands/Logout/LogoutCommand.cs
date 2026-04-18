using FluentValidation;
using MediatR;
using YuG.Application.Common;

namespace YuG.Application.Auth.Commands.Logout;

/// <summary>
/// 登出命令
/// </summary>
public class LogoutCommand : CommandBase<Unit>
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// 登出命令验证器
/// </summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>
    /// 初始化登出命令验证器
    /// </summary>
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("刷新令牌不能为空");
    }
}
