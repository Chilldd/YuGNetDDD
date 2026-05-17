using System.Text.Json;

namespace YuG.AI.Gateway.Middleware;

/// <summary>AI 网关异常处理中间件，统一处理 AI 调用相关异常并返回标准错误响应。</summary>
public class AiExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<AiExceptionHandlingMiddleware> _logger;

    /// <summary>初始化 <see cref="AiExceptionHandlingMiddleware"/> 实例。</summary>
    /// <param name="logger">日志记录器</param>
    public AiExceptionHandlingMiddleware(ILogger<AiExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>执行中间件逻辑，捕获异常并转换为标准错误响应。</summary>
    /// <param name="context">HTTP 上下文</param>
    /// <param name="next">下一个请求委托</param>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("provider", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "AI provider configuration error");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteErrorAsync(context, "Invalid AI provider configuration");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI provider request failed");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await WriteErrorAsync(context, "AI provider service unavailable");
        }
        catch (TaskCanceledException)
        {
            context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
            await WriteErrorAsync(context, "Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in AI Gateway");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteErrorAsync(context, "Internal server error");
        }
    }

    /// <summary>写入 JSON 格式的错误响应。</summary>
    /// <param name="context">HTTP 上下文</param>
    /// <param name="message">错误消息</param>
    private static async Task WriteErrorAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(json);
    }
}
