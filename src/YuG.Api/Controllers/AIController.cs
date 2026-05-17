using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Helpers;
using YuG.Application.AI.Chat.Common;
using YuG.Application.AI.Chat.Send;
using YuG.Application.AI.Chat.Stream;

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
}
