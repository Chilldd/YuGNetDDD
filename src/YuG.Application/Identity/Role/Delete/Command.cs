using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.Role.Delete;

/// <summary>
/// 删除角色命令
/// </summary>
public class DeleteRoleCommand : IRequest
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 删除角色命令验证器
/// </summary>
public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    /// <summary>
    /// 初始化删除角色命令验证器
    /// </summary>
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");
    }
}
