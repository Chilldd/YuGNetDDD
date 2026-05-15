using MediatR;
using YuG.Domain.Common.Interfaces;

namespace YuG.Application.Monitoring.Status.GetStatus;

/// <summary>
/// 获取系统状态查询处理器
/// </summary>
public class GetStatusHandler : IRequestHandler<GetStatusQuery, GetStatusResult>
{
    private readonly IServerInfoService _serverInfoService;
    private readonly IHealthCheckService _healthCheckService;

    /// <summary>
    /// 初始化获取系统状态查询处理器
    /// </summary>
    /// <param name="serverInfoService">服务器信息服务</param>
    /// <param name="healthCheckService">健康检查服务</param>
    public GetStatusHandler(IServerInfoService serverInfoService, IHealthCheckService healthCheckService)
    {
        _serverInfoService = serverInfoService;
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// 处理获取系统状态查询
    /// </summary>
    /// <param name="request">查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>系统状态结果</returns>
    public async Task<GetStatusResult> Handle(GetStatusQuery request, CancellationToken cancellationToken)
    {
        var serverInfo = await _serverInfoService.GetServerInfoAsync(cancellationToken);
        var health = await _healthCheckService.CheckAsync(cancellationToken);

        return new GetStatusResult
        {
            Server = serverInfo,
            Health = health,
            Timestamp = DateTime.UtcNow
        };
    }
}
