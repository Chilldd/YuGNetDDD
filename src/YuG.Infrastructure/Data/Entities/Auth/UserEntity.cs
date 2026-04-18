namespace YuG.Infrastructure.Data.Entities.Auth;

/// <summary>
/// 用户数据库实体（ORM 模型，包含审计属性）
/// </summary>
public class UserEntity : BaseEntity
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌集合（Owned Type）
    /// </summary>
    public List<RefreshTokenEntity> RefreshTokens { get; set; } = [];
}

