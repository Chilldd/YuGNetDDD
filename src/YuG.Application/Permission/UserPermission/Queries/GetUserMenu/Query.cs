using FluentValidation;
using MediatR;

namespace YuG.Application.Permission.UserPermission.Queries.GetUserMenu;

/// <summary>
/// 获取当前用户菜单查询
/// </summary>
public class GetUserMenuQuery : IRequest<GetUserMenuResult>
{
    /// <summary>
    /// 用户标识（由控制器从 JWT Claims 中提取并传入）
    /// </summary>
    public long UserId { get; init; }
}

/// <summary>
/// 获取当前用户菜单查询验证器
/// </summary>
public class GetUserMenuQueryValidator : AbstractValidator<GetUserMenuQuery>
{
    /// <summary>
    /// 初始化获取当前用户菜单查询验证器
    /// </summary>
    public GetUserMenuQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("用户标识无效");
    }
}
