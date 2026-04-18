namespace YuG.Application.DTOs.Responses;

/// <summary>
/// 登录响应
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// 过期时间（UTC）
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}
