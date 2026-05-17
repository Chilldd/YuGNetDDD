using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace YuG.AI.Gateway.Middleware;

/// <summary>
/// 按用户 ID 限流的策略。从请求体的 <c>userId</c> 字段获取用户标识，
/// 未传时回退到客户端 IP。
/// <br/><br/>
/// 当前配置：滑动窗口，每分钟 60 次配额，切分为 6 段（每 10 秒一段），
/// 每段配额 10 次，超限后最多排队 5 个，超排队的请求返回 429。
/// <br/>
/// 分区键格式：<c>user:{id}</c> 或 <c>ip:{address}</c>。
/// </summary>
public class UserRateLimiterPolicy : IRateLimiterPolicy<string>
{
    private static readonly SlidingWindowRateLimiterOptions _options = new()
    {
        PermitLimit = 60,
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 6,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 5,
    };

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var key = GetPartitionKey(httpContext);
        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => _options);
    }

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected =>
        (ctx, _) =>
        {
            ctx.HttpContext.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(new { error = "请求过于频繁，请稍后再试" });
            return new ValueTask(ctx.HttpContext.Response.WriteAsync(json, cancellationToken: _));
        };

    /// <summary>从请求体或 JWT 获取用户标识。</summary>
    private static string GetPartitionKey(HttpContext context)
    {
        // 优先从 JWT 的 NameIdentifier claim 获取用户 ID
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            return $"user:{userId}";

        // 回退到 IP
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }
}
