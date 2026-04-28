using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Permission.Resource.Update;

/// <summary>
/// 更新资源响应
/// </summary>
public record ResourceResult
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 资源编码（唯一，用于权限系统引用）
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 资源描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 资源类型（Menu/Api/Button）
    /// </summary>
    public string Type { get; init; } = "Api";

    /// <summary>
    /// HTTP 方法（仅 API 类型，GET/POST/PUT/DELETE）
    /// </summary>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// API 路径（仅 API 类型，如 /api/users）
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// 菜单图标（仅菜单类型）
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// 前端路由（仅菜单类型）
    /// </summary>
    public string? Route { get; init; }

    /// <summary>
    /// 组件路径（仅菜单类型）
    /// </summary>
    public string? Component { get; init; }

    /// <summary>
    /// 是否隐藏（仅菜单类型）
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// 菜单角标（仅菜单类型）
    /// </summary>
    public string? Badge { get; init; }

    /// <summary>
    /// 权限编码（仅按钮类型，如 user:create）
    /// </summary>
    public string? PermissionCode { get; init; }

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public long? ParentId { get; init; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 资源状态（Active/Disabled）
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
/// 更新资源命令
/// </summary>
public class UpdateResourceCommand : CommandBase<ResourceResult>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 资源编码（唯一，用于权限系统引用）
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 资源描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 资源类型（Menu/Api/Button）
    /// </summary>
    public string Type { get; init; } = "Api";

    /// <summary>
    /// HTTP 方法（仅 API 类型，GET/POST/PUT/DELETE）
    /// </summary>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// API 路径（仅 API 类型，如 /api/users）
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// 菜单图标（仅菜单类型）
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// 前端路由（仅菜单类型）
    /// </summary>
    public string? Route { get; init; }

    /// <summary>
    /// 组件路径（仅菜单类型）
    /// </summary>
    public string? Component { get; init; }

    /// <summary>
    /// 是否隐藏（仅菜单类型）
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// 菜单角标（仅菜单类型）
    /// </summary>
    public string? Badge { get; init; }

    /// <summary>
    /// 权限编码（仅按钮类型，如 user:create）
    /// </summary>
    public string? PermissionCode { get; init; }

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public long? ParentId { get; init; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 资源状态（Active/Disabled）
    /// </summary>
    public string Status { get; init; } = "Active";
}

/// <summary>
/// 更新资源命令验证器
/// </summary>
public class UpdateResourceCommandValidator : AbstractValidator<UpdateResourceCommand>
{
    /// <summary>
    /// 初始化更新资源命令验证器
    /// </summary>
    public UpdateResourceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("资源标识不能为空");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("资源名称不能为空")
            .MaximumLength(200).WithMessage("资源名称长度不能超过 200 个字符");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("资源编码不能为空")
            .MaximumLength(100).WithMessage("资源编码长度不能超过 100 个字符")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("资源编码只能包含字母、数字、下划线和短横线");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("资源描述长度不能超过 500 个字符");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("资源类型不能为空")
            .Must(type => new[] { "Menu", "Api", "Button" }.Contains(type))
            .WithMessage("资源类型必须是 Menu、Api 或 Button");

        // API 类型的条件验证
        When(x => x.Type == "Api", () =>
        {
            RuleFor(x => x.HttpMethod)
                .NotEmpty().WithMessage("API 类型的 HTTP 方法不能为空")
                .Must(method => new[] { "GET", "POST", "PUT", "DELETE" }.Contains(method!.ToUpperInvariant()))
                .WithMessage("HTTP 方法必须是 GET、POST、PUT 或 DELETE");

            RuleFor(x => x.Path)
                .NotEmpty().WithMessage("API 类型的路径不能为空")
                .MaximumLength(500).WithMessage("API 路径长度不能超过 500 个字符");
        });

        // 按钮类型的条件验证
        When(x => x.Type == "Button", () =>
        {
            RuleFor(x => x.PermissionCode)
                .NotEmpty().WithMessage("按钮类型的权限编码不能为空")
                .MaximumLength(100).WithMessage("权限编码长度不能超过 100 个字符");
        });

        // 菜单类型的条件验证
        When(x => x.Type == "Menu", () =>
        {
            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("菜单图标长度不能超过 100 个字符");

            RuleFor(x => x.Route)
                .MaximumLength(500).WithMessage("前端路由长度不能超过 500 个字符");

            RuleFor(x => x.Component)
                .MaximumLength(500).WithMessage("组件路径长度不能超过 500 个字符");

            RuleFor(x => x.Badge)
                .MaximumLength(50).WithMessage("菜单角标长度不能超过 50 个字符");
        });

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || new[] { "Active", "Disabled" }.Contains(status))
            .WithMessage("资源状态必须是 Active 或 Disabled");
    }
}
