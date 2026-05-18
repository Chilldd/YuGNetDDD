using FluentValidation;
using MediatR;

namespace YuG.Application.Identity.Role.Queries.GetUsers;

/// <summary>
/// 获取角色关联用户查询
/// </summary>
public class GetRoleUsersQuery : IRequest<GetRoleUsersResult>
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long RoleId { get; init; }
}

/// <summary>
/// 获取角色关联用户查询结果
/// </summary>
public record GetRoleUsersResult
{
    /// <summary>
    /// 用户列表
    /// </summary>
    public IReadOnlyList<RoleUserItem> Items { get; init; } = [];
}

/// <summary>
/// 角色关联用户项
/// </summary>
public record RoleUserItem
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
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// 获取角色关联用户查询验证器
/// </summary>
public class GetRoleUsersQueryValidator : AbstractValidator<GetRoleUsersQuery>
{
    /// <summary>
    /// 初始化获取角色关联用户查询验证器
    /// </summary>
    public GetRoleUsersQueryValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("角色标识必须大于 0");
    }
}
