using FluentValidation.Results;

namespace YuG.Application.Exceptions;

/// <summary>
/// 验证异常，当请求验证失败时抛出
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// 验证错误字典，键为属性名，值为错误消息列表
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// 初始化验证异常
    /// </summary>
    /// <param name="failures">验证失败列表</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("请求验证失败，请检查输入参数。")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
