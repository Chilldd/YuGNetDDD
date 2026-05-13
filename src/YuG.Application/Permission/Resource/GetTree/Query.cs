using FluentValidation;
using MediatR;

namespace YuG.Application.Permission.Resource.GetTree;

/// <summary>
/// 获取资源树查询
/// </summary>
public class GetResourceTreeQuery : IRequest<GetResourceTreeResult>
{
    /// <summary>
    /// 资源类型筛选（可选，Menu/Page/Api）
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// 是否只返回激活状态（可选）
    /// </summary>
    public bool? ActiveOnly { get; init; }
}

/// <summary>
/// 获取资源树查询验证器
/// </summary>
public class GetResourceTreeQueryValidator : AbstractValidator<GetResourceTreeQuery>
{
    /// <summary>
    /// 初始化获取资源树查询验证器
    /// </summary>
    public GetResourceTreeQueryValidator()
    {
        RuleFor(x => x.Type)
            .Must(type => string.IsNullOrEmpty(type)
                || new[] { "Menu", "Page", "Api" }.Contains(type))
            .WithMessage("资源类型必须是 Menu、Page 或 Api");
    }
}
