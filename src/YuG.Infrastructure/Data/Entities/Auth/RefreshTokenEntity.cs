namespace YuG.Infrastructure.Data.Entities.Auth;

/// <summary>
/// 刷新令牌数据库实体（Owned Type）
/// </summary>
public class RefreshTokenEntity
{
    /// <summary>
    /// 刷新令牌值
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 所属用户 ID（外键）
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 过期时间（UTC）
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已撤销
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
