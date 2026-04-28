using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.Role.Get;

/// <summary>
/// 获取单个角色查询
/// </summary>
public class GetRoleQuery : IRequest<GetRoleResult?>
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 获取单个角色查询结果
/// </summary>
public record GetRoleResult
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 角色编码
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 角色状态
    /// </summary>
    public string Status { get; init; } = "Active";

    /// <summary>
    /// 关联的资源标识列表
    /// </summary>
    public List<long> ResourceIds { get; init; } = [];

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// 获取单个角色查询验证器
/// </summary>
public class GetRoleQueryValidator : AbstractValidator<GetRoleQuery>
{
    /// <summary>
    /// 初始化获取单个角色查询验证器
    /// </summary>
    public GetRoleQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");
    }
}
