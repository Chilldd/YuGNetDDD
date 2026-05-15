using MediatR;
using YuG.Domain.Common.Interfaces;

namespace YuG.Application.Monitoring.Status.GetReady;

/// <summary>
/// 获取就绪检查查询处理器
/// </summary>
public class GetReadyHandler : IRequestHandler<GetReadyQuery, GetReadyResult>
{
    private readonly IHealthCheckService _healthCheckService;

    /// <summary>
    /// 初始化获取就绪检查查询处理器
    /// </summary>
    /// <param name="healthCheckService">健康检查服务</param>
    public GetReadyHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// 处理获取就绪检查查询
    /// </summary>
    /// <param name="request">查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>就绪检查结果</returns>
    public async Task<GetReadyResult> Handle(GetReadyQuery request, CancellationToken cancellationToken)
    {
        var health = await _healthCheckService.CheckAsync(cancellationToken);

        return new GetReadyResult
        {
            Status = health.Status.ToString(),
            Services = health.Services,
            Timestamp = DateTime.UtcNow
        };
    }
}
