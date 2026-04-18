using FluentValidation;
using YuG.Application.Common;
using YuG.Application.DTOs.Resource.Responses;

namespace YuG.Application.Queries.Resource.GetById;

/// <summary>
/// 获取资源查询
/// </summary>
public class GetResourceByIdQuery : QueryBase<ResourceResponse>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public Guid Id { get; init; }
}

/// <summary>
/// 获取资源查询验证器
/// </summary>
public class GetResourceByIdQueryValidator : AbstractValidator<GetResourceByIdQuery>
{
    /// <summary>
    /// 初始化获取资源查询验证器
    /// </summary>
    public GetResourceByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("资源标识不能为空");
    }
}
