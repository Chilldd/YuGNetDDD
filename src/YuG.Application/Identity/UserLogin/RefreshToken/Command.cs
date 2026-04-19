using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Identity.UserLogin.RefreshToken;

/// <summary>
/// 刷新令牌响应
/// </summary>
public record RefreshTokenResult
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// 过期时间（UTC）
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// 刷新令牌命令
/// </summary>
public class RefreshTokenCommand : CommandBase<RefreshTokenResult>
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// 刷新令牌命令验证器
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// 初始化刷新令牌命令验证器
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("刷新令牌不能为空");
    }
}
