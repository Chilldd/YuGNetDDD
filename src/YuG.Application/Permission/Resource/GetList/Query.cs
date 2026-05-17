using FluentValidation;
using MediatR;
using YuG.Common.Models;
using YuG.Domain.Permission.Enums;

namespace YuG.Application.Permission.Resource.GetList;

/// <summary>
/// 获取资源列表查询
/// </summary>
public class GetResourceListQuery : IRequest<PageResult<ResourceListItem>>
{
    /// <summary>
    /// 当前页码（从 1 开始）
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// 资源类型筛选（可选，Menu/Page/Api）
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// HTTP 方法筛选（可选）
    /// </summary>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// 父级资源标识筛选（可选）
    /// </summary>
    public long? ParentId { get; init; }

    /// <summary>
    /// 资源状态筛选（可选，Active/Disabled）
    /// </summary>
    public ResourceStatus? Status { get; init; }
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
        RuleFor(x => x.Type)
            .Must(type => string.IsNullOrEmpty(type)
                || new[] { "Menu", "Page", "Api" }.Contains(type))
            .WithMessage("资源类型必须是 Menu、Page 或 Api");

        RuleFor(x => x.HttpMethod)
            .Must(method => string.IsNullOrEmpty(method)
                || new[] { "GET", "POST", "PUT", "DELETE" }.Contains(method.ToUpperInvariant()))
            .WithMessage("HTTP 方法必须是 GET、POST、PUT 或 DELETE");
    }
}
