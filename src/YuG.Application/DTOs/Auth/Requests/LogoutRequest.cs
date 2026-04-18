namespace YuG.Application.DTOs.Auth.Requests;

/// <summary>
/// 登出请求
/// </summary>
public record LogoutRequest
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}
