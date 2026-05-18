namespace YuG.Application.Identity.User.DTOs;

/// <summary>
/// 用户响应
/// </summary>
public record UserResult
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
