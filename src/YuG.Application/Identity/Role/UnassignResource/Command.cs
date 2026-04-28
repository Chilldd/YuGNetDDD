using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.Role.UnassignResource;

/// <summary>
/// 从角色移除资源命令
/// </summary>
public class UnassignResourceCommand : IRequest
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long RoleId { get; init; }

    /// <summary>
    /// 资源标识
    /// </summary>
    public long ResourceId { get; init; }
}

/// <summary>
/// 从角色移除资源命令验证器
/// </summary>
public class UnassignResourceCommandValidator : AbstractValidator<UnassignResourceCommand>
{
    /// <summary>
    /// 初始化从角色移除资源命令验证器
    /// </summary>
    public UnassignResourceCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");

        RuleFor(x => x.ResourceId)
            .GreaterThan(0).WithMessage("资源标识必须大于 0");
    }
}
