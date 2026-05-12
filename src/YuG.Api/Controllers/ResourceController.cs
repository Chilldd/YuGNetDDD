using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Helpers;
using YuG.Application.Permission.Resource.Delete;
using YuG.Application.Permission.Resource.Get;
using YuG.Application.Permission.Resource.GetList;
using CreateResourceCommands = YuG.Application.Permission.Resource.Create;
using UpdateResourceCommands = YuG.Application.Permission.Resource.Update;
using ActivateResourceCommands = YuG.Application.Permission.Resource.Activate;
using DisableResourceCommands = YuG.Application.Permission.Resource.Disable;

namespace YuG.Api.Controllers;

/// <summary>
/// 资源管理控制器
/// </summary>
[ApiController]
[Authorize]
[Route("api/management/resources")]
public class ResourceController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化资源管理控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    public ResourceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取资源列表
    /// </summary>
    /// <param name="query">获取资源列表查询</param>
    /// <returns>资源列表</returns>
    /// <response code="200">查询成功</response>
    [HttpGet]
    [ApiDescription("获取资源列表")]
    [ProducesResponseType(typeof(GetResourceListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetResourceListResult>> Get(
        [FromQuery] GetResourceListQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="query">获取资源查询</param>
    /// <returns>资源详细信息</returns>
    /// <response code="200">查询成功</response>
    /// <response code="404">资源不存在</response>
    [HttpGet("{id}")]
    [ApiDescription("获取单个资源")]
    [ProducesResponseType(typeof(GetResourceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetResourceResult>> GetById([FromRoute] GetResourceQuery query)
    {
        var response = await _mediator.Send(query);
        if (response is null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    /// <summary>
    /// 创建资源
    /// </summary>
    /// <param name="command">创建资源命令</param>
    /// <returns>创建的资源</returns>
    /// <response code="201">创建成功</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [ApiDescription("创建资源")]
    [ProducesResponseType(typeof(CreateResourceCommands.ResourceResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateResourceCommands.ResourceResult>> Create([FromBody] CreateResourceCommands.CreateResourceCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// 更新资源
    /// </summary>
    /// <param name="command">更新资源命令</param>
    /// <returns>更新后的资源</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">资源不存在</response>
    [HttpPut]
    [ApiDescription("更新资源")]
    [ProducesResponseType(typeof(UpdateResourceCommands.ResourceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateResourceCommands.ResourceResult>> Update([FromBody] UpdateResourceCommands.UpdateResourceCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="id">资源标识</param>
    /// <returns>删除结果</returns>
    /// <response code="204">删除成功</response>
    /// <response code="404">资源不存在</response>
    [HttpDelete("{id}")]
    [ApiDescription("删除资源")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteResourceCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// 激活资源
    /// </summary>
    /// <param name="id">资源标识</param>
    /// <returns>激活后的资源</returns>
    /// <response code="200">激活成功</response>
    /// <response code="404">资源不存在</response>
    [HttpPost("{id}/activate")]
    [ApiDescription("激活资源")]
    [ProducesResponseType(typeof(ActivateResourceCommands.ResourceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivateResourceCommands.ResourceResult>> Activate(long id)
    {
        var command = new ActivateResourceCommands.ActivateResourceCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 禁用资源
    /// </summary>
    /// <param name="id">资源标识</param>
    /// <returns>禁用后的资源</returns>
    /// <response code="200">禁用成功</response>
    /// <response code="404">资源不存在</response>
    [HttpPost("{id}/disable")]
    [ApiDescription("禁用资源")]
    [ProducesResponseType(typeof(DisableResourceCommands.ResourceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisableResourceCommands.ResourceResult>> Disable(long id)
    {
        var command = new DisableResourceCommands.DisableResourceCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
