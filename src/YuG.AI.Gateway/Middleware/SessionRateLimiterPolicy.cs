using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace YuG.AI.Gateway.Middleware;

/// <summary>
/// 按 sessionId 限流的策略。未传 sessionId 时回退到客户端 IP。
/// <br/><br/>
/// 当前配置：滑动窗口，每分钟 60 次配额，切分为 6 段（每 10 秒一段），
/// 每段配额 10 次，超限后最多排队 5 个，超排队的请求返回 429。
/// <br/>
/// 分区键格式：<c>session:{id}</c> 或 <c>ip:{address}</c>。
/// </summary>
public class SessionRateLimiterPolicy : IRateLimiterPolicy<string>
{
    /// <summary>
    /// 滑动窗口限流选项
    /// <br/>
    /// - <c>PermitLimit = 60</c>：窗口内最大请求数
    /// - <c>Window = 1min</c>：窗口时长
    /// - <c>SegmentsPerWindow = 6</c>：窗口切分数，每段 10 秒，每段配额 10 次
    /// - <c>QueueLimit = 5</c>：超出后可排队等待的请求数
    /// </summary>
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

    /// <summary>从 <see cref="HttpContext.Items"/> 获取 sessionId，未找到时回退到客户端 IP。</summary>
    private static string GetPartitionKey(HttpContext context)
    {
        if (context.Items.TryGetValue("SessionId", out var sid) && sid is string sessionId)
            return $"session:{sessionId}";

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }
}
