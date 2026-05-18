using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Filters;
using YuG.Api.Helpers;
using YuG.Application.Monitoring.Status.Queries.GetHealth;
using YuG.Application.Monitoring.Status.Queries.GetReady;
using YuG.Application.Monitoring.Status.Queries.GetStatus;

namespace YuG.Api.Controllers.System;

/// <summary>
/// 系统运行状态控制器
/// </summary>
[ApiController]
[Route("api/system/status")]
public class StatusController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化系统运行状态控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    public StatusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取系统状态（服务器信息 + 健康检查）
    /// </summary>
    /// <returns>系统状态</returns>
    /// <response code="200">查询成功</response>
    [HttpGet]
    [ApiDescription("获取系统状态")]
    [Authorize(Policy = "status:getstatus")]
    [ProducesResponseType(typeof(GetStatusResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetStatusResult>> GetStatus()
    {
        var result = await _mediator.Send(new GetStatusQuery());
        return Ok(result);
    }

    /// <summary>
    /// 健康检查（用于负载均衡探活）
    /// </summary>
    /// <returns>健康检查结果</returns>
    /// <response code="200">服务健康</response>
    /// <response code="503">服务不健康</response>
    [HttpGet("health")]
    [AllowAnonymous]
    [IgnoreApiResponse]
    [ApiDescription("健康检查")]
    [ProducesResponseType(typeof(GetHealthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<GetHealthResult>> Health()
    {
        var result = await _mediator.Send(new GetHealthQuery());
        return result.Status == "Healthy" ? Ok(result) : StatusCode(503, result);
    }

    /// <summary>
    /// 就绪检查（包含各项依赖状态）
    /// </summary>
    /// <returns>就绪检查结果</returns>
    /// <response code="200">服务就绪</response>
    /// <response code="503">服务未就绪</response>
    [HttpGet("ready")]
    [AllowAnonymous]
    [IgnoreApiResponse]
    [ApiDescription("就绪检查")]
    [ProducesResponseType(typeof(GetReadyResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<GetReadyResult>> Ready()
    {
        var result = await _mediator.Send(new GetReadyQuery());
        return result.Status == "Healthy" ? Ok(result) : StatusCode(503, result);
    }
}
