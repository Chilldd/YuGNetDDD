using MediatR;

namespace YuG.Application.Identity.Role.GetList;

/// <summary>
/// 获取角色列表查询
/// </summary>
public class GetRoleListQuery : IRequest<GetRoleListResult>
{
}

/// <summary>
/// 获取角色列表查询结果
/// </summary>
public record GetRoleListResult
{
    /// <summary>
    /// 角色列表项
    /// </summary>
    public List<RoleListItem> Items { get; init; } = [];

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; init; }
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
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
