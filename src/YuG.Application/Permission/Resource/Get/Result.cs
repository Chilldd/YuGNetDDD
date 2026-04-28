namespace YuG.Application.Permission.Resource.Get;

/// <summary>
/// 获取资源响应
/// </summary>
public record GetResourceResult
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
    /// 资源编码（唯一，用于权限系统引用）
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 资源描述
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 资源类型（Menu/Api/Button）
    /// </summary>
    public string Type { get; init; } = "Api";

    /// <summary>
    /// HTTP 方法（仅 API 类型，GET/POST/PUT/DELETE）
    /// </summary>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// API 路径（仅 API 类型，如 /api/users）
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// 菜单图标（仅菜单类型）
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// 前端路由（仅菜单类型）
    /// </summary>
    public string? Route { get; init; }

    /// <summary>
    /// 组件路径（仅菜单类型）
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
    /// 权限编码（仅按钮类型，如 user:create）
    /// </summary>
    public string? PermissionCode { get; init; }

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public long? ParentId { get; init; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 资源状态（Active/Disabled）
    /// </summary>
    public string Status { get; init; } = "Active";

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
