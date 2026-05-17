using MediatR;
using YuG.Common.Interfaces;

namespace YuG.Application.Monitoring.Status.GetStatus;

/// <summary>
/// 获取系统状态查询
/// </summary>
public class GetStatusQuery : IRequest<GetStatusResult>
{
}

/// <summary>
/// 系统状态结果
/// </summary>
public class GetStatusResult
{
    /// <summary>
    /// 服务器信息
    /// </summary>
    public required ServerInfo Server { get; init; }

    /// <summary>
    /// 健康检查结果
    /// </summary>
    public HealthCheckResult? Health { get; init; }

    /// <summary>
    /// 查询时间戳
    /// </summary>
    public DateTime Timestamp { get; init; }
}
