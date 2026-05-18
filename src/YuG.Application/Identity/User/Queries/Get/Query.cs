using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.User.Queries.Get;

/// <summary>
/// 获取单个用户查询结果
/// </summary>
public record GetUserResult
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 用户状态
    /// </summary>
    public string Status { get; init; } = "Active";

    /// <summary>
    /// 关联的角色标识列表
    /// </summary>
    public List<long> RoleIds { get; init; } = [];

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// 获取单个用户查询
/// </summary>
public class GetUserQuery : IRequest<GetUserResult?>
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }
}

/// <summary>
/// 获取单个用户查询验证器
/// </summary>
public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    /// <summary>
    /// 初始化获取单个用户查询验证器
    /// </summary>
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户标识必须大于 0");
    }
}
