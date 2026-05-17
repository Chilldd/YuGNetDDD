namespace YuG.Common.Interfaces;

/// <summary>
/// 健康状态枚举
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// 健康
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// 亚健康（部分依赖不可用）
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// 不健康
    /// </summary>
    Unhealthy = 2
}

/// <summary>
/// 服务健康状态
/// </summary>
public record ServiceHealth
{
    /// <summary>
    /// 服务名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 健康状态
    /// </summary>
    public HealthStatus Status { get; init; }

    /// <summary>
    /// 响应延迟（毫秒）
    /// </summary>
    public long LatencyMs { get; init; }

    /// <summary>
    /// 详细信息（可选）
    /// </summary>
    public string? Details { get; init; }
}

/// <summary>
/// 健康检查结果
/// </summary>
public record HealthCheckResult
{
    /// <summary>
    /// 整体健康状态
    /// </summary>
    public HealthStatus Status { get; init; }

    /// <summary>
    /// 各项服务健康详情
    /// </summary>
    public IReadOnlyList<ServiceHealth> Services { get; init; } = [];
}

/// <summary>
/// 健康检查服务接口
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// 执行健康检查
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>健康检查结果</returns>
    Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default);
}
