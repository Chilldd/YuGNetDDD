namespace YuG.Application.DTOs.Auth.Requests;

/// <summary>
/// 刷新令牌请求
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}
