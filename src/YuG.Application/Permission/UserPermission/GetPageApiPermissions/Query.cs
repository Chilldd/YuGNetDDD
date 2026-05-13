using FluentValidation;
using MediatR;

namespace YuG.Application.Permission.UserPermission.GetPageApiPermissions;

/// <summary>
/// 获取页面 API 权限查询
/// </summary>
public class GetPageApiPermissionsQuery : IRequest<GetPageApiPermissionsResult>
{
    /// <summary>
    /// 用户标识（由控制器从 JWT Claims 中提取并传入）
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 页面资源标识
    /// </summary>
    public long PageId { get; init; }
}

/// <summary>
/// 获取页面 API 权限查询验证器
/// </summary>
public class GetPageApiPermissionsQueryValidator : AbstractValidator<GetPageApiPermissionsQuery>
{
    /// <summary>
    /// 初始化获取页面 API 权限查询验证器
    /// </summary>
    public GetPageApiPermissionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("用户标识无效");

        RuleFor(x => x.PageId)
            .GreaterThan(0).WithMessage("页面标识无效");
    }
}
