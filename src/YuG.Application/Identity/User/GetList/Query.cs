using MediatR;
using YuG.Common.Models;

namespace YuG.Application.Identity.User.GetList;

/// <summary>
/// 获取用户列表查询
/// </summary>
public class GetUserListQuery : IRequest<GetUserListResult>
{
}

/// <summary>
/// 获取用户列表查询结果
/// </summary>
public record GetUserListResult : PageResult<UserListItem>;

/// <summary>
/// 用户列表项
/// </summary>
public record UserListItem
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
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
