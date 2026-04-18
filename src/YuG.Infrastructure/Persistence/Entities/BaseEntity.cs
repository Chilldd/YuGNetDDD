namespace YuG.Infrastructure.Persistence.Entities;

/// <summary>
/// ORM 实体基类（与 Domain 聚合根完全独立）
/// 只负责数据库映射，不包含任何业务逻辑
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
