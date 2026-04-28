using YuG.Domain.Common;
using YuG.Domain.Common.Interfaces;
using YuG.Domain.Identity.ValueObjects;

namespace YuG.Domain.Identity.Entities;

/// <summary>
/// 用户领域对象
/// </summary>
public class User : AggregateRoot
{
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<Role> _roles = [];

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// 刷新令牌集合（只读）
    /// </summary>
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>
    /// 角色集合（多对多，只读）
    /// </summary>
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    /// <summary>
    /// 创建用户（用于ORM）
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="passwordHash">密码哈希</param>
    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// 设置用户角色（覆盖模式：删除旧角色，保存新角色）
    /// </summary>
    /// <param name="roles">新角色集合</param>
    public void SetRoles(IEnumerable<Role> roles)
    {
        _roles.Clear();

        foreach (var role in roles)
        {
            if (!_roles.Any(r => r.Id == role.Id))
            {
                _roles.Add(role);
            }
        }
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="passwordHasher">密码哈希服务</param>
    /// <param name="password">明文密码</param>
    /// <returns>密码是否正确</returns>
    public bool VerifyPassword(IPasswordHasher passwordHasher, string password)
    {
        return passwordHasher.Verify(password, PasswordHash);
    }

    /// <summary>
    /// 添加刷新令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    public void AddRefreshToken(RefreshToken refreshToken)
    {
        // 清理过期的令牌
        _refreshTokens.RemoveAll(t => t.ExpiresAt < DateTime.UtcNow || t.IsRevoked);

        // 添加新令牌
        _refreshTokens.Add(refreshToken);
    }

    /// <summary>
    /// 撤销刷新令牌
    /// </summary>
    /// <param name="token">令牌值</param>
    /// <returns>是否成功撤销</returns>
    public bool RevokeRefreshToken(string token)
    {
        var index = _refreshTokens.FindIndex(t => t.Token == token);
        if (index < 0)
        {
            return false;
        }

        var refreshToken = _refreshTokens[index];
        _refreshTokens[index] = refreshToken.Revoke();
        return true;
    }

    /// <summary>
    /// 获取有效的刷新令牌
    /// </summary>
    /// <param name="token">令牌值</param>
    /// <returns>刷新令牌，不存在或无效则返回 null</returns>
    public RefreshToken? GetValidRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsValid());
    }
}
