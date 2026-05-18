using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Permission.Resource.Commands.Disable;

/// <summary>
/// 禁用资源命令
/// </summary>
public class DisableResourceCommand : CommandBase<ResourceResult>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public long Id { get; init; }
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
