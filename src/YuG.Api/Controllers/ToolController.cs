using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Helpers;
using YuG.Application.Permission.Resource.SyncApiEndpoints;

namespace YuG.Api.Controllers;

/// <summary>
/// 开发工具控制器
/// </summary>
[ApiController]
[Route("api/tool")]
public class ToolController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApiEndpointScanner _endpointScanner;

    /// <summary>
    /// 初始化工具控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    /// <param name="endpointScanner">API 端点扫描器</param>
    public ToolController(
        IMediator mediator,
        IApiEndpointScanner endpointScanner)
    {
        _mediator = mediator;
        _endpointScanner = endpointScanner;
    }

    /// <summary>
    /// 同步扫描到的 API 端点到资源表
    /// </summary>
    /// <returns>同步结果统计</returns>
    /// <response code="200">同步成功</response>
    [AllowAnonymous]
    [HttpPost("sync-api-resources")]
    [ProducesResponseType(typeof(SyncApiEndpointsResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<SyncApiEndpointsResult>> SyncApiResources()
    {
        var scanResult = _endpointScanner.Scan();
        var command = new SyncApiEndpointsCommand
        {
            Controllers = scanResult.Controllers,
            Endpoints = scanResult.Endpoints
        };
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
