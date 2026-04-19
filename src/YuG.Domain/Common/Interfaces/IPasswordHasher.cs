namespace YuG.Domain.Common.Interfaces;

/// <summary>
/// 密码哈希服务接口
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 对密码进行哈希
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>密码哈希</returns>
    string Hash(string password);

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="hash">密码哈希</param>
    /// <returns>密码是否正确</returns>
    bool Verify(string password, string hash);
}
