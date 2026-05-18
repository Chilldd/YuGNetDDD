using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YuG.AI.Gateway.Models.Requests;
using YuG.AI.Gateway.Models.Responses;
using YuG.AI.Gateway.Services;

namespace YuG.AI.Gateway.Controllers;

/// <summary>JSON 序列化配置，使用 camelCase 以与下游客户端约定一致。</summary>
internal static class SseJsonOptions
{
    internal static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}

/// <summary>聊天补全接口控制器。</summary>
[ApiController]
[EnableRateLimiting("Chat")]
[Route("api/v1/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    /// <summary>初始化 <see cref="ChatController"/> 实例。</summary>
    /// <param name="chatService">聊天服务</param>
    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>基于完整消息历史的聊天补全。消息列表包含 system/user/assistant 所有历史。</summary>
    /// <param name="request">聊天请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>AI 回复</returns>
    [HttpPost]
    public async Task<ActionResult<ChatReplyResponse>> Chat(
        [FromBody] ChatRequest request, CancellationToken ct)
    {
        if (request.Messages is null || request.Messages.Count == 0)
            return BadRequest(new { error = "Messages cannot be empty" });

        var result = await _chatService.ChatAsync(request.Messages, ct);
        return Ok(result);
    }

    /// <summary>基于完整消息历史的流式聊天补全，使用 SSE 协议推送响应。</summary>
    /// <param name="request">聊天请求</param>
    /// <param name="ct">取消令牌</param>
    [HttpPost("stream")]
    public async Task Stream(
        [FromBody] ChatRequest request, CancellationToken ct)
    {
        if (request.Messages is null || request.Messages.Count == 0)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(new { error = "Messages cannot be empty" }, ct);
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var chatId = $"chatcmpl-{Guid.NewGuid():N}";

        try
        {
            await foreach (var delta in _chatService.ChatStreamAsync(request.Messages, ct))
            {
                var json = delta.Type switch
                {
                    "tool_call" => JsonSerializer.Serialize(new
                    {
                        type = "tool_call",
                        tool_call = delta.ToolCall,
                    }, SseJsonOptions.Default),
                    "tool_result" => JsonSerializer.Serialize(new
                    {
                        type = "tool_result",
                        tool_result = delta.ToolResult,
                    }, SseJsonOptions.Default),
                    "usage" => JsonSerializer.Serialize(new
                    {
                        type = "usage",
                        usage = delta.Usage,
                    }, SseJsonOptions.Default),
                    _ => JsonSerializer.Serialize(new
                    {
                        type = "delta",
                        content = delta.Content,
                    }, SseJsonOptions.Default)
                };
                await Response.WriteAsync($"data: {json}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (Exception ex)
        {
            // 流开始后响应头已发送，不能抛给中间件改 StatusCode。
            // 改为发一条 SSE error 事件通知客户端，然后吃掉异常（中间件已无法处理）。
            var errorJson = JsonSerializer.Serialize(new { type = "error" }, SseJsonOptions.Default);
            await Response.WriteAsync($"data: {errorJson}\n\n", ct);
            await Response.Body.FlushAsync(ct);

            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ChatController>>();
            logger.LogError(ex, "Stream error after response started");
        }

        var done = JsonSerializer.Serialize(new
        {
            type = "done",
            id = chatId,
        }, SseJsonOptions.Default);
        await Response.WriteAsync($"data: {done}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
