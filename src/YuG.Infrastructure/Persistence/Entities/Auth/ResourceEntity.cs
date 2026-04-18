namespace YuG.Infrastructure.Persistence.Entities;

/// <summary>
/// 资源数据库实体（ORM 模型，包含审计属性）
/// </summary>
public class ResourceEntity : BaseEntity
{
    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 资源编码（唯一，用于权限系统引用）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 资源描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// HTTP 方法（GET/POST/PUT/DELETE）
    /// </summary>
    public string HttpMethod { get; set; } = "GET";

    /// <summary>
    /// API 路径（如 /api/users）
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 资源状态（Active/Disabled）
    /// </summary>
    public string Status { get; set; } = "Active";
}
