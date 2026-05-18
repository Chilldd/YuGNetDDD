using MediatR;
using YuG.Common.Interfaces;

namespace YuG.Application.Monitoring.Status.Queries.GetHealth;

/// <summary>
/// 获取健康检查查询处理器
/// </summary>
public class GetHealthHandler : IRequestHandler<GetHealthQuery, GetHealthResult>
{
    private readonly IHealthCheckService _healthCheckService;

    /// <summary>
    /// 初始化获取健康检查查询处理器
    /// </summary>
    /// <param name="healthCheckService">健康检查服务</param>
    public GetHealthHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// 处理获取健康检查查询
    /// </summary>
    /// <param name="request">查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>健康检查结果</returns>
    public async Task<GetHealthResult> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var health = await _healthCheckService.CheckAsync(cancellationToken);

        return new GetHealthResult
        {
            Status = health.Status.ToString(),
            Timestamp = DateTime.UtcNow
        };
    }
}
