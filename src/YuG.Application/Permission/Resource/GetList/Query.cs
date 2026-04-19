using FluentValidation;
using MediatR;

namespace YuG.Application.Permission.Resource.GetList;

/// <summary>
/// 获取资源列表查询
/// </summary>
public class GetResourceListQuery : IRequest<GetResourceListResult>
{
    /// <summary>
    /// HTTP 方法筛选（可选）
    /// </summary>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// 父级资源标识筛选（可选）
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// 是否只返回激活状态（可选）
    /// </summary>
    public bool? ActiveOnly { get; init; }
}

/// <summary>
/// 获取资源列表查询验证器
/// </summary>
public class GetResourceListQueryValidator : AbstractValidator<GetResourceListQuery>
{
    /// <summary>
    /// 初始化获取资源列表查询验证器
    /// </summary>
    public GetResourceListQueryValidator()
    {
        RuleFor(x => x.HttpMethod)
            .Must(method => string.IsNullOrEmpty(method)
                || new[] { "GET", "POST", "PUT", "DELETE" }.Contains(method.ToUpperInvariant()))
            .WithMessage("HTTP 方法必须是 GET、POST、PUT 或 DELETE");
    }
}
