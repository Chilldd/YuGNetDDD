using FluentValidation;
using MediatR;
using YuG.Application.Common;

namespace YuG.Application.AI.Session.Commands.Rename;

/// <summary>重命名会话命令。</summary>
public class RenameSessionCommand : CommandBase<Unit>
{
    /// <summary>会话 ID。</summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>新标题。</summary>
    public string Title { get; init; } = string.Empty;
}

/// <summary>重命名会话命令验证器。</summary>
public class RenameSessionCommandValidator : AbstractValidator<RenameSessionCommand>
{
    /// <summary>初始化验证规则。</summary>
    public RenameSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("会话 ID 不能为空");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("会话标题不能为空")
            .MaximumLength(200).WithMessage("会话标题长度不能超过 200 个字符");
    }
}
