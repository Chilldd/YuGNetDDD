using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.DTOs;

/// <summary>
/// 角色响应
/// </summary>
public record RoleResult
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
    /// 角色编码（唯一）
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 角色状态（Active/Disabled）
    /// </summary>
    public string Status { get; init; } = "Active";

    /// <summary>
    /// 是否为系统内置角色
    /// </summary>
    public bool IsSystem { get; init; }

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// 将角色实体映射为响应结果
    /// </summary>
    internal static RoleResult FromEntity(RoleEntity role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Code = role.Code,
        Description = role.Description,
        Status = role.Status.ToString(),
        IsSystem = role.IsSystem,
        CreatedAt = role.CreatedAt,
        UpdatedAt = role.UpdatedAt
    };
}
