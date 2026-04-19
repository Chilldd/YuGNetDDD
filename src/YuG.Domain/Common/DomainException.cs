namespace YuG.Domain.Common;

/// <summary>
/// 领域异常基类，用于表示业务规则违反
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// 初始化领域异常
    /// </summary>
    /// <param name="message">异常消息</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// 初始化领域异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
