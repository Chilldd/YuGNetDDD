using YuG.Application.Common;
using YuG.Common.Models;

namespace YuG.Application.Identity.Role.GetList;

/// <summary>
/// 获取角色列表查询
/// </summary>
public class GetRoleListQuery : PagedQuery<PageResult<RoleListItem>>
{
}

/// <summary>
/// 角色列表项
/// </summary>
public record RoleListItem
{
    /// <summary>
    /// 角色标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 角色编码
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 角色状态
    /// </summary>
    public string Status { get; init; } = "Active";

    /// <summary>
    /// 是否为系统内置角色
    /// </summary>
    public bool IsSystem { get; init; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
