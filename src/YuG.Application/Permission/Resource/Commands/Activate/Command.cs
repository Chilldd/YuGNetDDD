using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Permission.Resource.Commands.Activate;

/// <summary>
/// 激活资源命令
/// </summary>
public class ActivateResourceCommand : CommandBase<ResourceResult>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public long Id { get; init; }
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
