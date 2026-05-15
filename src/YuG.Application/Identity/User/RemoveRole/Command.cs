using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.User.RemoveRole;

/// <summary>
/// 移除用户角色命令
/// </summary>
public class RemoveUserRoleCommand : IRequest
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 角色标识
    /// </summary>
    public long RoleId { get; init; }
}

/// <summary>
/// 移除用户角色命令验证器
/// </summary>
public class RemoveUserRoleCommandValidator : AbstractValidator<RemoveUserRoleCommand>
{
    /// <summary>
    /// 初始化移除用户角色命令验证器
    /// </summary>
    public RemoveUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("用户标识必须大于 0");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");
    }
}
