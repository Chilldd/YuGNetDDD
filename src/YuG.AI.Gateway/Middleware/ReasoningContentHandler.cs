using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace YuG.AI.Gateway.Middleware;

/// <summary>
/// 处理 DeepSeek 思考模式下的 <c>reasoning_content</c> 回传。
/// SK 1.76 的 OpenAI connector 在工具调用多轮交互中不会自动携带该字段回 DeepSeek API，
/// 导致 HTTP 400。此 handler 在 HTTP 层拦截请求/响应，自动提取并回填。
/// </summary>
internal sealed class ReasoningContentHandler : DelegatingHandler
{
    private static readonly ConcurrentDictionary<string, CacheEntry> ReasoningCache = new();
    private static readonly JsonWriterOptions JsonWriterOptions = new() { Indented = false };
    private static readonly MediaTypeHeaderValue JsonMediaType = new("application/json") { CharSet = Encoding.UTF8.WebName };

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    /// <summary>缓存条目，带时间戳用于过期清理。</summary>
    private sealed record CacheEntry(string Content, DateTime CreatedAt);

    /// <summary>供 ChatService 手动缓存 streaming 响应中提取的 reasoning_content。</summary>
    public static void CacheReasoningContent(string callId, string content)
    {
        ReasoningCache[callId] = new CacheEntry(content, DateTime.UtcNow);
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // ===== 请求路径：注入缓存中的 reasoning_content =====
        if (request.Content?.Headers.ContentType?.MediaType == "application/json")
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            var modified = InjectReasoningContent(body);
            if (modified != body)
            {
                request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(modified));
                request.Content.Headers.ContentType = JsonMediaType;
            }
        }

        // ===== 发送请求并获取原始响应 =====
        var response = await base.SendAsync(request, cancellationToken);

        // ===== 响应路径：提取并缓存 reasoning_content =====
        if (response.Content?.Headers.ContentType?.MediaType == "application/json")
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            ExtractAndCacheReasoningContent(body);
        }

        return response;
    }

    /// <summary>
    /// 扫描 assistant 消息中的 <c>tool_calls</c>，匹配缓存中的 <c>reasoning_content</c> 并注入。
    /// </summary>
    private static string InjectReasoningContent(string jsonBody)
    {
        using var doc = JsonDocument.Parse(jsonBody);
        if (!doc.RootElement.TryGetProperty("messages", out var messages))
            return jsonBody;

        using var stream = new MemoryStream(jsonBody.Length + 512);
        using var writer = new Utf8JsonWriter(stream, JsonWriterOptions);
        writer.WriteStartObject();

        var hasInjection = false;

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.Name == "messages")
            {
                writer.WriteStartArray("messages");
                foreach (var msg in messages.EnumerateArray())
                {
                    var role = msg.TryGetProperty("role", out var r) ? r.GetString() : null;
                    if (role == "assistant" && msg.TryGetProperty("tool_calls", out var toolCalls))
                    {
                        var reasoningContent = FindCachedReasoning(toolCalls);
                        if (reasoningContent is not null)
                        {
                            WriteMessageWithReasoning(writer, msg, reasoningContent);
                            hasInjection = true;
                            continue;
                        }
                    }

                    msg.WriteTo(writer);
                }
                writer.WriteEndArray();
            }
            else
            {
                prop.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        return hasInjection ? Encoding.UTF8.GetString(stream.ToArray()) : jsonBody;
    }

    private static string? FindCachedReasoning(JsonElement toolCalls)
    {
        var now = DateTime.UtcNow;

        foreach (var tc in toolCalls.EnumerateArray())
        {
            if (!tc.TryGetProperty("id", out var idProp))
                continue;

            var callId = idProp.GetString();
            if (callId is null)
                continue;

            if (ReasoningCache.TryGetValue(callId, out var entry))
            {
                // 过期条目视为未命中，触发后续按需清理
                if (now - entry.CreatedAt > CacheTtl)
                {
                    TryCleanupOne(callId);
                    continue;
                }

                return entry.Content;
            }
        }

        return null;
    }

    /// <summary>写入消息的所有属性，外加 <c>reasoning_content</c>。</summary>
    private static void WriteMessageWithReasoning(Utf8JsonWriter writer, JsonElement msg, string reasoningContent)
    {
        writer.WriteStartObject();
        foreach (var prop in msg.EnumerateObject())
        {
            prop.WriteTo(writer);
        }
        writer.WriteString("reasoning_content", reasoningContent);
        writer.WriteEndObject();
    }

    /// <summary>
    /// 从响应中提取 assistant 消息的 <c>reasoning_content</c>，按 <c>tool_call.id</c> 缓存。
    /// 写入新条目时顺便清扫过期数据。
    /// </summary>
    private static void ExtractAndCacheReasoningContent(string jsonBody)
    {
        using var doc = JsonDocument.Parse(jsonBody);

        if (!doc.RootElement.TryGetProperty("choices", out var choices))
            return;

        var now = DateTime.UtcNow;
        var cachedCount = 0;

        foreach (var choice in choices.EnumerateArray())
        {
            if (!choice.TryGetProperty("message", out var msg))
                continue;

            if (!msg.TryGetProperty("reasoning_content", out var rc))
                continue;

            var reasoningContent = rc.GetString();
            if (string.IsNullOrEmpty(reasoningContent))
                continue;

            if (!msg.TryGetProperty("tool_calls", out var toolCalls))
                continue;

            foreach (var tc in toolCalls.EnumerateArray())
            {
                if (tc.TryGetProperty("id", out var idProp))
                {
                    var callId = idProp.GetString();
                    if (!string.IsNullOrEmpty(callId))
                    {
                        ReasoningCache[callId] = new CacheEntry(reasoningContent, now);
                        cachedCount++;
                    }
                }
            }
        }

        // 写入新条目后按需触发批量清扫
        if (cachedCount > 0)
        {
            SweepExpired();
        }
    }

    /// <summary>尝试移除单个过期条目。</summary>
    private static void TryCleanupOne(string key)
    {
        if (ReasoningCache.TryGetValue(key, out var entry) && DateTime.UtcNow - entry.CreatedAt > CacheTtl)
        {
            ReasoningCache.TryRemove(key, out _);
        }
    }

    /// <summary>批量移除所有过期条目（每次写入后触发，平摊清扫成本）。</summary>
    private static void SweepExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in ReasoningCache)
        {
            if (now - kvp.Value.CreatedAt > CacheTtl)
            {
                ReasoningCache.TryRemove(kvp.Key, out _);
            }
        }
    }
}
