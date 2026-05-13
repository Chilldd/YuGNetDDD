using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Identity.User.Create;

/// <summary>
/// 创建用户响应
/// </summary>
public record UserResult
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// 创建用户命令
/// </summary>
public class CreateUserCommand : CommandBase<UserResult>
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
/// 创建用户命令验证器
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>
    /// 初始化创建用户命令验证器
    /// </summary>
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(50).WithMessage("用户名长度不能超过 50 个字符")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("用户名只能包含字母、数字和下划线");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码长度不能少于 6 个字符")
            .MaximumLength(100).WithMessage("密码长度不能超过 100 个字符");
    }
}
