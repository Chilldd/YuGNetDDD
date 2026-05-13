namespace YuG.Application.Permission.UserPermission.GetUserMenu;

/// <summary>
/// 用户菜单树节点
/// </summary>
public record UserMenuTreeItem
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 资源编码
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// 前端路由
    /// </summary>
    public string? Route { get; init; }

/// <summary>
    /// 是否隐藏
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// 菜单角标
    /// </summary>
    public string? Badge { get; init; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 权限编码（页面类型有效）
    /// </summary>
    public string? PermissionCode { get; init; }

    /// <summary>
    /// 子页面列表
    /// </summary>
    public List<UserMenuTreeItem> Children { get; set; } = [];
}

/// <summary>
/// 获取用户菜单响应
/// </summary>
public record GetUserMenuResult
{
    /// <summary>
    /// 菜单树列表
    /// </summary>
    public IReadOnlyList<UserMenuTreeItem> Items { get; init; } = [];
}
