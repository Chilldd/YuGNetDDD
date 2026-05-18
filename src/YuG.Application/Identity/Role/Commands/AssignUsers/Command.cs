using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.Role.Commands.AssignUsers;

/// <summary>
/// 给角色分配用户命令（追加模式，已有用户跳过）
/// </summary>
public class AssignRoleUsersCommand : IRequest
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long RoleId { get; init; }

    /// <summary>
    /// 用户标识列表
    /// </summary>
    public List<long> UserIds { get; init; } = [];
}

/// <summary>
/// 给角色分配用户命令验证器
/// </summary>
public class AssignRoleUsersCommandValidator : AbstractValidator<AssignRoleUsersCommand>
{
    /// <summary>
    /// 初始化给角色分配用户命令验证器
    /// </summary>
    public AssignRoleUsersCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");

        RuleFor(x => x.UserIds)
            .NotNull().WithMessage("用户标识列表不能为空");
    }
}
