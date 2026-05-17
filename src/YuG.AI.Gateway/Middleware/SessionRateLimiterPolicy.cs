using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace YuG.AI.Gateway.Middleware;

/// <summary>按 sessionId 限流的策略。未传 sessionId 时回退到客户端 IP。</summary>
public class SessionRateLimiterPolicy : IRateLimiterPolicy<string>
{
    private static readonly FixedWindowRateLimiterOptions _options = new()
    {
        PermitLimit = 3,
        Window = TimeSpan.FromMinutes(1),
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 5,
    };

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var key = GetPartitionKey(httpContext);
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => _options);
    }

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected =>
        (ctx, _) =>
        {
            ctx.HttpContext.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(new { error = "请求过于频繁，请稍后再试" });
            return new ValueTask(ctx.HttpContext.Response.WriteAsync(json, cancellationToken: _));
        };

    private static string GetPartitionKey(HttpContext context)
    {
        if (context.Items.TryGetValue("SessionId", out var sid) && sid is string sessionId)
            return $"session:{sessionId}";

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }
}
