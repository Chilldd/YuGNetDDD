namespace YuG.Domain.Interfaces;

/// <summary>
/// JWT 令牌服务接口
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// 生成 JWT 访问令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="username">用户名</param>
    /// <returns>JWT 访问令牌</returns>
    string GenerateAccessToken(Guid userId, string username);

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <param name="expirationDays">过期天数</param>
    /// <returns>刷新令牌值</returns>
    string GenerateRefreshToken(int expirationDays = 7);

    /// <summary>
    /// 从 JWT 令牌中提取用户ID
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>用户ID</returns>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// 验证令牌是否有效
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>令牌是否有效</returns>
    bool ValidateToken(string token);
}
