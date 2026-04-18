using FluentValidation;
using YuG.Application.Common;
using YuG.Application.DTOs.Auth.Responses;

namespace YuG.Application.Commands.Auth.Login;

/// <summary>
/// 登录命令
/// </summary>
public class LoginCommand : CommandBase<LoginResponse>
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// 登录命令验证器
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// 初始化登录命令验证器
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(50).WithMessage("用户名长度不能超过50个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空");
    }
}
