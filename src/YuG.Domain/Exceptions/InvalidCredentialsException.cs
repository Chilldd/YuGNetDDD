namespace YuG.Domain.Exceptions;

/// <summary>
/// 无效凭据异常，当用户名或密码不正确时抛出
/// </summary>
public class InvalidCredentialsException : DomainException
{
    /// <summary>
    /// 初始化无效凭据异常
    /// </summary>
    public InvalidCredentialsException()
        : base("用户名或密码不正确")
    {
    }

    /// <summary>
    /// 初始化无效凭据异常
    /// </summary>
    /// <param name="message">错误消息</param>
    public InvalidCredentialsException(string message)
        : base(message)
    {
    }
}
