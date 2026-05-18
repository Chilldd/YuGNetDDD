using FluentValidation;
using YuG.Application.Common;
using YuG.Application.Identity.Role.DTOs;

namespace YuG.Application.Identity.Role.Commands.Activate;

/// <summary>
/// 激活角色命令
/// </summary>
public class ActivateRoleCommand : CommandBase<RoleResult>
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 激活角色命令验证器
/// </summary>
public class ActivateRoleCommandValidator : AbstractValidator<ActivateRoleCommand>
{
    /// <summary>
    /// 初始化激活角色命令验证器
    /// </summary>
    public ActivateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");
    }
}
