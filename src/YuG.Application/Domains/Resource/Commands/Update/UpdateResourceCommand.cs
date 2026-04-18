using FluentValidation;
using YuG.Application.Common;
using YuG.Application.DTOs.Resource.Responses;

namespace YuG.Application.Resource.Commands.Update;

/// <summary>
/// 更新资源命令
/// </summary>
public class UpdateResourceCommand : CommandBase<ResourceResponse>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public Guid Id { get; init; }

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
    /// HTTP 方法（GET/POST/PUT/DELETE）
    /// </summary>
    public string HttpMethod { get; init; } = "GET";

    /// <summary>
    /// API 路径（如 /api/users）
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public Guid? ParentId { get; init; }

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

        RuleFor(x => x.HttpMethod)
            .NotEmpty().WithMessage("HTTP 方法不能为空")
            .Must(method => new[] { "GET", "POST", "PUT", "DELETE" }.Contains(method.ToUpperInvariant()))
            .WithMessage("HTTP 方法必须是 GET、POST、PUT 或 DELETE");

        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("API 路径不能为空")
            .MaximumLength(500).WithMessage("API 路径长度不能超过 500 个字符");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || new[] { "Active", "Disabled" }.Contains(status))
            .WithMessage("资源状态必须是 Active 或 Disabled");
    }
}
