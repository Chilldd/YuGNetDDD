using System.Net;
using System.Text.Json;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Common;

namespace YuG.Api.Middleware;

/// <summary>
/// 全局异常处理中间件，捕获未处理异常并返回标准化错误响应
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    /// <summary>
    /// 初始化异常处理中间件
    /// </summary>
    /// <param name="next">下一个中间件</param>
    /// <param name="logger">日志记录器</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 处理 HTTP 请求
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = validationEx.Message,
                    Errors = validationEx.Errors
                }
            ),
            NotFoundException => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = exception.Message
                }
            ),
            DomainException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = exception.Message
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "服务器内部错误，请稍后重试。"
                }
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "未处理的异常");
        }
        else
        {
            _logger.LogWarning(exception, "业务异常: {Message}", exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private class ErrorResponse
    {
        /// <summary>
        /// HTTP 状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 验证错误详情
        /// </summary>
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}
