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
    private static readonly ConcurrentDictionary<string, string> ReasoningCache = new();
    private static readonly JsonWriterOptions JsonWriterOptions = new() { Indented = false };
    private static readonly MediaTypeHeaderValue JsonMediaType = new("application/json") { CharSet = Encoding.UTF8.WebName };

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
            var extracted = ExtractAndCacheReasoningContent(body);
            if (extracted != body)
            {
                response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(extracted));
                response.Content.Headers.ContentType = JsonMediaType;
            }
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
        foreach (var tc in toolCalls.EnumerateArray())
        {
            if (tc.TryGetProperty("id", out var idProp))
            {
                var callId = idProp.GetString();
                if (callId is not null && ReasoningCache.TryGetValue(callId, out var cached))
                    return cached;
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
    /// 返回修改后的 JSON（原样，此阶段无修改），但用于清理已消费的缓存项。
    /// </summary>
    private static string ExtractAndCacheReasoningContent(string jsonBody)
    {
        using var doc = JsonDocument.Parse(jsonBody);

        if (!doc.RootElement.TryGetProperty("choices", out var choices))
            return jsonBody;

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

            // 按每个 tool_call.id 缓存 reasoning_content
            foreach (var tc in toolCalls.EnumerateArray())
            {
                if (tc.TryGetProperty("id", out var idProp))
                {
                    var callId = idProp.GetString();
                    if (!string.IsNullOrEmpty(callId))
                    {
                        ReasoningCache[callId] = reasoningContent;
                    }
                }
            }
        }

        return jsonBody;
    }
}
