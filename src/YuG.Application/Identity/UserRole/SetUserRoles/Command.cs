using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.UserRole.SetUserRoles;

/// <summary>
/// 设置用户角色命令（覆盖模式：删除旧角色，保存新角色）
/// </summary>
public class SetUserRolesCommand : IRequest
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 角色标识列表
    /// </summary>
    public List<long> RoleIds { get; init; } = [];
}

/// <summary>
/// 设置用户角色命令验证器
/// </summary>
public class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    /// <summary>
    /// 初始化设置用户角色命令验证器
    /// </summary>
    public SetUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("用户标识必须大于 0");

        RuleFor(x => x.RoleIds)
            .NotNull().WithMessage("角色标识列表不能为空");
    }
}
