namespace YuG.Common.Models;

/// <summary>
/// 统一 API 响应
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 状态码，0 表示成功，非 0 表示错误
    /// </summary>
    public int Code { get; init; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// 验证错误详情
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; init; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="message">成功消息</param>
    /// <returns>统一成功响应</returns>
    public static ApiResponse Ok(object? data = null, string message = "success")
        => new() { Code = 0, Message = message, Data = data };

    /// <summary>
    /// 创建错误响应
    /// </summary>
    /// <param name="code">错误码</param>
    /// <param name="message">错误消息</param>
    /// <param name="errors">验证错误详情</param>
    /// <returns>统一错误响应</returns>
    public static ApiResponse Fail(int code, string message, IDictionary<string, string[]>? errors = null)
        => new() { Code = code, Message = message, Errors = errors };
}
