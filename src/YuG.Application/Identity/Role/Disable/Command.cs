using FluentValidation;
using YuG.Application.Common;
using RoleResult = YuG.Application.Identity.Role.Create.RoleResult;

namespace YuG.Application.Identity.Role.Disable;

/// <summary>
/// 禁用角色命令
/// </summary>
public class DisableRoleCommand : CommandBase<RoleResult>
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 禁用角色命令验证器
/// </summary>
public class DisableRoleCommandValidator : AbstractValidator<DisableRoleCommand>
{
    /// <summary>
    /// 初始化禁用角色命令验证器
    /// </summary>
    public DisableRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");
    }
}
