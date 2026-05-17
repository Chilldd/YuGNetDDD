using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Helpers;
using YuG.Application.AI.Chat.Common;
using YuG.Application.AI.Chat.Send;
using YuG.Application.AI.Chat.Stream;
using YuG.Application.AI.Session.Delete;
using YuG.Application.AI.Session.GetList;
using YuG.Application.AI.Session.Rename;

namespace YuG.Api.Controllers;

/// <summary>AI 能力控制器，提供对话、Agent 等 AI 相关接口。</summary>
[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>初始化聊天控制器。</summary>
    /// <param name="mediator">MediatR 发送器</param>
    public AIController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>发送聊天消息。</summary>
    /// <param name="command">聊天命令</param>
    /// <returns>AI 回复结果</returns>
    /// <response code="200">回复成功</response>
    /// <response code="400">请求参数校验失败</response>
    [HttpPost]
    [ApiDescription("发送聊天消息")]
    [ProducesResponseType(typeof(ChatReplyResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatReplyResult>> Chat([FromBody] ChatCommand command)
    {
        var result = await _mediator.Send(command, HttpContext.RequestAborted);
        return Ok(result);
    }

    /// <summary>流式聊天（SSE 协议）。</summary>
    /// <param name="command">流式聊天命令</param>
    /// <response code="200">SSE 事件流</response>
    /// <response code="400">请求参数校验失败</response>
    [HttpPost("stream")]
    [ApiDescription("流式聊天（SSE）")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task Stream([FromBody] StreamChatCommand command)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        try
        {
            var stream = await _mediator.Send(command, HttpContext.RequestAborted);

            await foreach (var delta in stream.WithCancellation(HttpContext.RequestAborted))
            {
                var json = JsonSerializer.Serialize(delta, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                await Response.WriteAsync($"data: {json}\n\n", HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
        }
        catch (OperationCanceledException)
        {
            // 客户端断开连接，正常结束
        }
    }

    /// <summary>获取当前用户的会话列表。</summary>
    /// <returns>会话列表</returns>
    /// <response code="200">获取成功</response>
    [HttpGet("sessions")]
    [ApiDescription("获取会话列表")]
    [ProducesResponseType(typeof(GetSessionListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetSessionListResult>> GetSessions()
    {
        var result = await _mediator.Send(new GetSessionListQuery(), HttpContext.RequestAborted);
        return Ok(result);
    }

    /// <summary>删除指定会话。</summary>
    /// <param name="sessionId">会话 ID</param>
    /// <response code="204">删除成功</response>
    /// <response code="404">会话不存在</response>
    [HttpDelete("sessions/{sessionId}")]
    [ApiDescription("删除会话")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteSession(string sessionId)
    {
        await _mediator.Send(new DeleteSessionCommand { SessionId = sessionId }, HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>重命名会话。</summary>
    /// <param name="sessionId">会话 ID</param>
    /// <param name="command">重命名命令</param>
    /// <response code="204">重命名成功</response>
    /// <response code="404">会话不存在</response>
    /// <response code="400">参数校验失败</response>
    [HttpPut("sessions/{sessionId}/rename")]
    [ApiDescription("重命名会话")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RenameSession(string sessionId, [FromBody] RenameSessionCommand command)
    {
        var cmd = new RenameSessionCommand { SessionId = sessionId, Title = command.Title };
        await _mediator.Send(cmd, HttpContext.RequestAborted);
        return NoContent();
    }
}
