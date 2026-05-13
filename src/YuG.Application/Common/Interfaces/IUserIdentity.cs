namespace YuG.Application.Common.Interfaces;

/// <summary>
/// 当前用户身份信息接口（从 JWT Claims 中提取）
/// </summary>
public interface IUserIdentity
{
    /// <summary>
    /// 当前用户标识
    /// </summary>
    long UserId { get; }

    /// <summary>
    /// 当前用户名
    /// </summary>
    string Username { get; }

    /// <summary>
    /// 当前用户的角色编码列表
    /// </summary>
    IReadOnlyList<string> Roles { get; }
}
