using YuG.Domain.Interfaces;

namespace YuG.Infrastructure.Services;

/// <summary>
/// BCrypt 密码哈希服务实现
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    /// <summary>
    /// 对密码进行哈希
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>密码哈希</returns>
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="hash">密码哈希</param>
    /// <returns>密码是否正确</returns>
    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
