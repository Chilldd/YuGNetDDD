namespace YuG.Application.Permission.Resource.GetTree;

/// <summary>
/// 资源树节点
/// </summary>
public record ResourceTreeItem
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
    /// 资源描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 资源类型（Menu/Page/Api）
    /// </summary>
    public string Type { get; init; } = "Api";

    /// <summary>
    /// HTTP 方法（仅 API 类型）
    /// </summary>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// API 路径（仅 API 类型）
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// 菜单图标（仅菜单类型）
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// 前端路由（菜单/页面类型有效）
    /// </summary>
    public string? Route { get; init; }

    /// <summary>
    /// 组件路径（菜单/页面类型有效）
    /// </summary>
    public string? Component { get; init; }

    /// <summary>
    /// 是否隐藏（仅菜单类型）
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// 菜单角标（仅菜单类型）
    /// </summary>
    public string? Badge { get; init; }

    /// <summary>
    /// 权限编码（页面/API 类型有效）
    /// </summary>
    public string? PermissionCode { get; init; }

    /// <summary>
    /// 父级资源标识
    /// </summary>
    public long? ParentId { get; init; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 资源状态
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 子资源列表
    /// </summary>
    public List<ResourceTreeItem> Children { get; set; } = [];
}

/// <summary>
/// 获取资源树响应
/// </summary>
public record GetResourceTreeResult
{
    /// <summary>
    /// 根节点列表
    /// </summary>
    public IReadOnlyList<ResourceTreeItem> Items { get; init; } = [];
}
