using FluentValidation;
using YuG.Application.Common;
using YuG.Application.DTOs.Auth.Responses;

namespace YuG.Application.Auth.Commands.RefreshToken;

/// <summary>
/// 刷新令牌命令
/// </summary>
public class RefreshTokenCommand : CommandBase<RefreshTokenResponse>
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
