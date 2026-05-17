using System.Text.Json;

namespace YuG.AI.Gateway.Middleware;

/// <summary>读取请求体中的 sessionId，存入 <see cref="HttpContext.Items"/>，供限流策略使用。</summary>
public class RequestBodyParsingMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method == HttpMethod.Post.Method
            && context.Request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("sessionId", out var el)
                    && el.ValueKind == JsonValueKind.String)
                {
                    var sid = el.GetString();
                    if (!string.IsNullOrWhiteSpace(sid))
                        context.Items["SessionId"] = sid;
                }
            }
            catch (JsonException)
            {
            }
        }

        await next(context);
    }
}
