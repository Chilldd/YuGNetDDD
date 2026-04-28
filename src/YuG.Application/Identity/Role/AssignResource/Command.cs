using FluentValidation;
using YuG.Application.Common;
using RoleResult = YuG.Application.Identity.Role.Create.RoleResult;

namespace YuG.Application.Identity.Role.AssignResource;

/// <summary>
/// 给角色分配资源命令（覆盖模式）
/// </summary>
public class AssignResourceCommand : CommandBase<RoleResult>
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long RoleId { get; init; }

    /// <summary>
    /// 资源标识列表
    /// </summary>
    public List<long> ResourceIds { get; init; } = [];
}

/// <summary>
/// 给角色分配资源命令验证器
/// </summary>
public class AssignResourceCommandValidator : AbstractValidator<AssignResourceCommand>
{
    /// <summary>
    /// 初始化给角色分配资源命令验证器
    /// </summary>
    public AssignResourceCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");

        RuleFor(x => x.ResourceIds)
            .NotNull().WithMessage("资源标识列表不能为空");
    }
}
