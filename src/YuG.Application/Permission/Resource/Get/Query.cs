using FluentValidation;
using MediatR;

namespace YuG.Application.Permission.Resource.Get;

/// <summary>
/// 获取资源查询
/// </summary>
public class GetResourceQuery : IRequest<GetResourceResult?>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 获取资源查询验证器
/// </summary>
public class GetResourceQueryValidator : AbstractValidator<GetResourceQuery>
{
    /// <summary>
    /// 初始化获取资源查询验证器
    /// </summary>
    public GetResourceQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("资源标识不能为空");
    }
}
