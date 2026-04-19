using FluentValidation;
using MediatR;
using YuG.Application.Common;

namespace YuG.Application.Domains.Resource.Commands.Delete;

/// <summary>
/// 删除资源命令
/// </summary>
public class DeleteResourceCommand : CommandBase<Unit>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public Guid Id { get; init; }
}

/// <summary>
/// 删除资源命令验证器
/// </summary>
public class DeleteResourceCommandValidator : AbstractValidator<DeleteResourceCommand>
{
    /// <summary>
    /// 初始化删除资源命令验证器
    /// </summary>
    public DeleteResourceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("资源标识不能为空");
    }
}
