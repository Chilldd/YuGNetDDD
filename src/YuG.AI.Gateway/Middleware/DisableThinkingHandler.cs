using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace YuG.AI.Gateway.Middleware;

/// <summary>
/// 向 DeepSeek 请求体中注入 <c>thinking: {type: disabled}</c>，用于禁用思考模式。
/// DeepSeek 启用思考模式后会在响应中返回 <c>reasoning_content</c> 字段，
/// 但 SK 1.76 的 OpenAI connector 在后续工具调用轮次中不会回传该字段，导致 400 错误。
/// </summary>
internal sealed class DisableThinkingHandler : DelegatingHandler
{
    private static readonly JsonWriterOptions JsonWriterOptions = new() { Indented = false };
    private static readonly MediaTypeHeaderValue JsonMediaType = new("application/json") { CharSet = Encoding.UTF8.WebName };

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is null ||
            request.Content.Headers.ContentType?.MediaType != "application/json")
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var body = await request.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(body);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, JsonWriterOptions);

        writer.WriteStartObject();

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            prop.WriteTo(writer);
        }

        // 注入 thinking: {type: disabled}
        writer.WriteStartObject("thinking");
        writer.WriteString("type", "disabled");
        writer.WriteEndObject();

        writer.WriteEndObject();
        writer.Flush();

        request.Content = new ByteArrayContent(stream.ToArray());
        request.Content.Headers.ContentType = JsonMediaType;

        return await base.SendAsync(request, cancellationToken);
    }
}
