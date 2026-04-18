namespace YuG.Application.DTOs.Resource.Requests;

/// <summary>
/// 创建资源请求
/// </summary>
public record CreateResourceRequest
{
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
    /// HTTP 方法（GET/POST/PUT/DELETE）
    /// </summary>
    public string HttpMethod { get; init; } = "GET";

    /// <summary>
    /// API 路径（如 /api/users）
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// 资源状态（Active/Disabled）
    /// </summary>
    public string Status { get; init; } = "Active";
}
