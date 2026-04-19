namespace YuG.Domain.Identity.ValueObjects;

/// <summary>
/// 刷新令牌值对象（不可变）
/// </summary>
public record RefreshToken
{
    /// <summary>
    /// 空值（用于创建基础实例）
    /// </summary>
    public static RefreshToken Empty => new();

    /// <summary>
    /// 令牌值
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// 是否已撤销
    /// </summary>
    public bool IsRevoked { get; init; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 撤销令牌（返回新的不可变实例）
    /// </summary>
    /// <returns>已撤销的新令牌实例</returns>
    public RefreshToken Revoke()
    {
        if (IsRevoked)
        {
            return this;
        }
        return this with { IsRevoked = true };
    }

    /// <summary>
    /// 检查令牌是否有效
    /// </summary>
    /// <returns>令牌是否有效（未撤销且未过期）</returns>
    public bool IsValid()
    {
        return !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
