using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using YuG.Common.Interfaces;
using YuG.Infrastructure.Persistence;

namespace YuG.Infrastructure.Services;

/// <summary>
/// 健康检查服务实现
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICache _cache;

    /// <summary>
    /// 初始化健康检查服务
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="cache">缓存服务</param>
    public HealthCheckService(ApplicationDbContext dbContext, ICache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var services = new List<ServiceHealth>
        {
            await CheckDatabaseAsync(cancellationToken),
            await CheckCacheAsync(cancellationToken)
        };

        var overallStatus = services.Count == 0
            ? HealthStatus.Healthy
            : services.Any(s => s.Status == HealthStatus.Unhealthy)
                ? HealthStatus.Unhealthy
                : services.Any(s => s.Status == HealthStatus.Degraded)
                    ? HealthStatus.Degraded
                    : HealthStatus.Healthy;

        return new HealthCheckResult
        {
            Status = overallStatus,
            Services = services
        };
    }

    /// <summary>
    /// 检查数据库连接
    /// </summary>
    private async Task<ServiceHealth> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            sw.Stop();

            return new ServiceHealth
            {
                Name = "Database",
                Status = canConnect ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                LatencyMs = sw.ElapsedMilliseconds,
                Details = canConnect ? null : "数据库无法连接"
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ServiceHealth
            {
                Name = "Database",
                Status = HealthStatus.Unhealthy,
                LatencyMs = sw.ElapsedMilliseconds,
                Details = $"数据库连接异常：{ex.Message}"
            };
        }
    }

    /// <summary>
    /// 检查缓存服务
    /// </summary>
    private async Task<ServiceHealth> CheckCacheAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var key = "__health_check__";
            var exists = await _cache.ExistsAsync(key, cancellationToken);
            sw.Stop();

            return new ServiceHealth
            {
                Name = "Cache",
                Status = HealthStatus.Healthy,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ServiceHealth
            {
                Name = "Cache",
                Status = HealthStatus.Degraded,
                LatencyMs = sw.ElapsedMilliseconds,
                Details = $"缓存服务异常：{ex.Message}"
            };
        }
    }
}
