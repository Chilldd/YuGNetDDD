using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Permission.Resource.Commands.Move;

/// <summary>
/// 移动资源命令
/// </summary>
public class MoveResourceCommand : CommandBase<ResourceResult>
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 新的父级资源标识（null 表示移到根级别）
    /// </summary>
    public long? ParentId { get; init; }
}

/// <summary>
/// 移动资源命令验证器
/// </summary>
public class MoveResourceCommandValidator : AbstractValidator<MoveResourceCommand>
{
    /// <summary>
    /// 初始化移动资源命令验证器
    /// </summary>
    public MoveResourceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("资源标识不能为空");
    }
}
