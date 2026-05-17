using Microsoft.AspNetCore.Mvc;
using YuG.AI.Gateway.Models.Requests;
using YuG.AI.Gateway.Models.Responses;
using YuG.AI.Gateway.Services;

namespace YuG.AI.Gateway.Controllers;

/// <summary>聊天补全接口控制器。</summary>
[ApiController]
[Route("api/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ISessionService _sessionService;

    /// <summary>初始化 <see cref="ChatController"/> 实例。</summary>
    /// <param name="chatService">聊天服务</param>
    /// <param name="sessionService">会话管理服务</param>
    public ChatController(IChatService chatService, ISessionService sessionService)
    {
        _chatService = chatService;
        _sessionService = sessionService;
    }

    /// <summary>基于会话的聊天补全。服务端自动管理对话上下文，客户端只需传递消息。</summary>
    /// <param name="request">聊天请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>回复与 sessionId</returns>
    [HttpPost]
    public async Task<ActionResult<ChatReplyResponse>> Chat(
        [FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message cannot be empty" });

        var (history, sessionId) = _sessionService.GetOrCreateSession(request.SessionId);
        var result = await _chatService.ChatWithHistoryAsync(history, request.Message, ct);
        result.SessionId = sessionId;

        return Ok(result);
    }

    /// <summary>基于会话的流式聊天补全，使用 SSE 协议推送响应。服务端自动管理对话上下文。</summary>
    /// <param name="request">聊天请求</param>
    /// <param name="ct">取消令牌</param>
    [HttpPost("stream")]
    public async Task Stream(
        [FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(new { error = "Message cannot be empty" }, ct);
            return;
        }

        var (history, sessionId) = _sessionService.GetOrCreateSession(request.SessionId);

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var chatId = $"chatcmpl-{Guid.NewGuid():N}";

        await foreach (var delta in _chatService.ChatStreamWithHistoryAsync(history, request.Message, ct))
        {
            var json = delta.Type switch
            {
                "usage" => System.Text.Json.JsonSerializer.Serialize(new
                {
                    type = "usage",
                    usage = delta.Usage,
                    sessionId
                }),
                _ => System.Text.Json.JsonSerializer.Serialize(new
                {
                    type = "delta",
                    content = delta.Content,
                    sessionId
                })
            };
            await Response.WriteAsync($"data: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }

        var done = System.Text.Json.JsonSerializer.Serialize(new
        {
            type = "done",
            id = chatId,
            sessionId
        });
        await Response.WriteAsync($"data: {done}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }

    /// <summary>清空指定会话的历史记录。</summary>
    /// <param name="sessionId">会话 ID</param>
    [HttpDelete("session/{sessionId}")]
    public ActionResult ClearSession(string sessionId)
    {
        _sessionService.ClearSession(sessionId);
        return Ok(new { message = "Session cleared" });
    }
}