using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace YuG.AI.Gateway.Middleware;

/// <summary>
/// 向 DeepSeek 请求体中注入 <c>thinking: {type: disabled}</c>，禁用思考模式。
/// 思考模式启用时 DeepSeek 会返回 <c>reasoning_content</c>，
/// 但 SK 序列化不支持将该字段回传，导致工具调用多轮交互时 HTTP 400。
/// 禁用思考模式可彻底绕开此问题。
/// </summary>
internal sealed class DisableThinkingHandler : DelegatingHandler
{
    private static readonly JsonWriterOptions JsonWriterOptions = new() { Indented = false };
    private static readonly MediaTypeHeaderValue JsonMediaType = new("application/json") { CharSet = Encoding.UTF8.WebName };

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content?.Headers.ContentType?.MediaType == "application/json")
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);

            using var doc = JsonDocument.Parse(body);

            using var stream = new MemoryStream(body.Length + 64);
            using var writer = new Utf8JsonWriter(stream, JsonWriterOptions);
            writer.WriteStartObject();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                prop.WriteTo(writer);
            }

            writer.WriteStartObject("thinking");
            writer.WriteString("type", "disabled");
            writer.WriteEndObject();

            writer.WriteEndObject();
            writer.Flush();

            request.Content = new ByteArrayContent(stream.ToArray());
            request.Content.Headers.ContentType = JsonMediaType;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
