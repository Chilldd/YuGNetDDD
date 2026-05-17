using FluentValidation;
using MediatR;
using YuG.Application.Common;

namespace YuG.Application.AI.Session.Delete;

/// <summary>删除会话命令。</summary>
public class DeleteSessionCommand : CommandBase<Unit>
{
    /// <summary>会话 ID。</summary>
    public string SessionId { get; init; } = string.Empty;
}

/// <summary>删除会话命令验证器。</summary>
public class DeleteSessionCommandValidator : AbstractValidator<DeleteSessionCommand>
{
    /// <summary>初始化验证规则。</summary>
    public DeleteSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("会话 ID 不能为空");
    }
}
