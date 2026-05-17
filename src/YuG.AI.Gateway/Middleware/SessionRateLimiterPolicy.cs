using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace YuG.AI.Gateway.Middleware;

/// <summary>按 sessionId 限流的策略。未传 sessionId 时回退到客户端 IP。</summary>
public class SessionRateLimiterPolicy : IRateLimiterPolicy<string>
{
    private static readonly FixedWindowRateLimiterOptions _options = new()
    {
        PermitLimit = 60,
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
        // 先尝试从请求体读取 sessionId
        if (context.Request.Method == HttpMethod.Post.Method
            && context.Request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = reader.ReadToEnd();
                context.Request.Body.Position = 0;

                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("sessionId", out var el)
                    && el.ValueKind == JsonValueKind.String)
                {
                    var sid = el.GetString();
                    if (!string.IsNullOrWhiteSpace(sid))
                        return $"session:{sid}";
                }
            }
            catch
            {
                // body 解析失败，回退到 IP
            }
        }

        // 回退：按客户端 IP
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }
}
