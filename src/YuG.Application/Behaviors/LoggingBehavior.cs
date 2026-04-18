using MediatR;
using Microsoft.Extensions.Logging;

namespace YuG.Application.Behaviors;

/// <summary>
/// MediatR 管道行为，记录请求处理日志
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// 初始化日志行为
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 处理请求并记录日志
    /// </summary>
    /// <param name="request">请求实例</param>
    /// <param name="next">下一个管道行为</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("处理请求: {RequestName} {@Request}", requestName, request);

        var response = await next();

        _logger.LogInformation("请求完成: {RequestName}", requestName);

        return response;
    }
}
