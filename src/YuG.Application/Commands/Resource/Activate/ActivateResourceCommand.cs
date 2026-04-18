using FluentValidation;
using MediatR;
using YuG.Application.Common;
using YuG.Application.DTOs.Resource.Responses;

namespace YuG.Application.Commands.Resource.Activate;

/// <summary>
/// 激活资源命令
/// </summary>
public class ActivateResourceCommand : CommandBase<ResourceResponse>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public Guid Id { get; init; }
}

/// <summary>
/// 激活资源命令验证器
/// </summary>
public class ActivateResourceCommandValidator : AbstractValidator<ActivateResourceCommand>
{
    /// <summary>
    /// 初始化激活资源命令验证器
    /// </summary>
    public ActivateResourceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("资源标识不能为空");
    }
}
