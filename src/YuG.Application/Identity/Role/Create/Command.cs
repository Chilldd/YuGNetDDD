using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Identity.Role.Create;

/// <summary>
/// 角色响应
/// </summary>
public record RoleResult
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
    /// 角色编码（唯一）
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 角色状态（Active/Disabled）
    /// </summary>
    public string Status { get; init; } = "Active";

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
/// 创建角色命令
/// </summary>
public class CreateRoleCommand : CommandBase<RoleResult>
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 角色编码（唯一）
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// 创建角色命令验证器
/// </summary>
public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    /// <summary>
    /// 初始化创建角色命令验证器
    /// </summary>
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("角色名称不能为空")
            .MaximumLength(100).WithMessage("角色名称长度不能超过 100 个字符");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("角色编码不能为空")
            .MaximumLength(100).WithMessage("角色编码长度不能超过 100 个字符")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("角色编码只能包含字母、数字、下划线和短横线");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("角色描述长度不能超过 500 个字符");
    }
}
