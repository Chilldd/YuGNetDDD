using MediatR;

namespace YuG.Application.Monitoring.Status.GetHealth;

/// <summary>
/// 获取健康检查查询
/// </summary>
public class GetHealthQuery : IRequest<GetHealthResult>
{
}

/// <summary>
/// 健康检查结果（简洁版）
/// </summary>
public class GetHealthResult
{
    /// <summary>
    /// 健康状态
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 查询时间戳
    /// </summary>
    public DateTime Timestamp { get; init; }
}
