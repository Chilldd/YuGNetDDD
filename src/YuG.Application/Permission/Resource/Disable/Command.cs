using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Permission.Resource.Disable;

/// <summary>
/// 禁用资源响应
/// </summary>
public record ResourceResult
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
/// 禁用资源命令
/// </summary>
public class DisableResourceCommand : CommandBase<ResourceResult>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public Guid Id { get; init; }
}

/// <summary>
/// 禁用资源命令验证器
/// </summary>
public class DisableResourceCommandValidator : AbstractValidator<DisableResourceCommand>
{
    /// <summary>
    /// 初始化禁用资源命令验证器
    /// </summary>
    public DisableResourceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("资源标识不能为空");
    }
}
