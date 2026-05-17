using MediatR;
using YuG.Common.Interfaces;

namespace YuG.Application.Monitoring.Status.GetReady;

/// <summary>
/// 获取就绪检查查询
/// </summary>
public class GetReadyQuery : IRequest<GetReadyResult>
{
}

/// <summary>
/// 就绪检查结果（包含各项依赖状态）
/// </summary>
public class GetReadyResult
{
    /// <summary>
    /// 整体就绪状态
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 各项服务健康详情
    /// </summary>
    public IReadOnlyList<ServiceHealth> Services { get; init; } = [];

    /// <summary>
    /// 查询时间戳
    /// </summary>
    public DateTime Timestamp { get; init; }
}
