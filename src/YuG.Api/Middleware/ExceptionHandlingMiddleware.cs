using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Refit;
using YuG.Application.Common.Exceptions;
using YuG.Common.Models;
using YuG.Domain.Common;

namespace YuG.Api.Middleware;

/// <summary>
/// 全局异常处理中间件，捕获未处理异常并返回标准化错误响应
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// 初始化异常处理中间件
    /// </summary>
    /// <param name="next">下一个中间件</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="jsonOptions">全局 JSON 序列化选项</param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IOptions<JsonOptions> jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
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
                ApiResponse.Fail(40001, validationEx.Message, validationEx.Errors)
            ),
            NotFoundException => (
                HttpStatusCode.NotFound,
                ApiResponse.Fail(40400, exception.Message)
            ),
            DomainException => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(40000, exception.Message)
            ),
            ApiException apiEx => (
                apiEx.StatusCode,
                ApiResponse.Fail((int)apiEx.StatusCode * 100, apiEx.Message)
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse.Fail(40100, "未授权访问")
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse.Fail(50000, "服务器内部错误，请稍后重试。")
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

        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
