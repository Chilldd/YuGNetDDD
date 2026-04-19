namespace YuG.Application.Permission.Resource.GetList;

/// <summary>
/// 资源列表项
/// </summary>
public record ResourceListItem
{
    /// <summary>
    /// 资源标识
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 资源编码
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// HTTP 方法
    /// </summary>
    public string HttpMethod { get; init; } = string.Empty;

    /// <summary>
    /// API 路径
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// 父级资源标识
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 资源状态
    /// </summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// 获取资源列表响应
/// </summary>
public record GetResourceListResult
{
    /// <summary>
    /// 资源列表
    /// </summary>
    public IReadOnlyList<ResourceListItem> Items { get; init; } = [];

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; init; }
}
