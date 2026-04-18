namespace YuG.Application.DTOs.Requests;

/// <summary>
/// 登录请求
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
