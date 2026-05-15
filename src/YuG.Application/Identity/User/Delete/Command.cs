using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.User.Delete;

/// <summary>
/// 删除用户命令
/// </summary>
public class DeleteUserCommand : IRequest
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 删除用户命令验证器
/// </summary>
public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    /// <summary>
    /// 初始化删除用户命令验证器
    /// </summary>
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户标识必须大于 0");
    }
}
