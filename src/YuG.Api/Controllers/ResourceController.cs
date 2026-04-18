using MediatR;
using Microsoft.AspNetCore.Mvc;
using YuG.Application.Commands.Resource.Activate;
using YuG.Application.Commands.Resource.Create;
using YuG.Application.Commands.Resource.Delete;
using YuG.Application.Commands.Resource.Disable;
using YuG.Application.Commands.Resource.Update;
using YuG.Application.DTOs.Resource.Requests;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Application.Queries.Resource.GetAll;
using YuG.Application.Queries.Resource.GetById;

namespace YuG.Api.Controllers;

/// <summary>
/// 资源管理控制器
/// </summary>
[ApiController]
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
    /// <param name="httpMethod">HTTP 方法筛选（可选）</param>
    /// <param name="parentId">父级资源标识筛选（可选）</param>
    /// <param name="activeOnly">是否只返回激活状态（可选）</param>
    /// <returns>资源列表</returns>
    /// <response code="200">查询成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(ResourceListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceListResponse>> Get(
        [FromQuery] string? httpMethod = null,
        [FromQuery] Guid? parentId = null,
        [FromQuery] bool? activeOnly = null)
    {
        var query = new GetAllResourcesQuery
        {
            HttpMethod = httpMethod,
            ParentId = parentId,
            ActiveOnly = activeOnly
        };

        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="id">资源标识</param>
    /// <returns>资源详细信息</returns>
    /// <response code="200">查询成功</response>
    /// <response code="404">资源不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceResponse>> GetById(Guid id)
    {
        var query = new GetResourceByIdQuery { Id = id };
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// 创建资源
    /// </summary>
    /// <param name="request">创建资源请求</param>
    /// <returns>创建的资源</returns>
    /// <response code="201">创建成功</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResourceResponse>> Create([FromBody] CreateResourceRequest request)
    {
        var command = new CreateResourceCommand
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            HttpMethod = request.HttpMethod,
            Path = request.Path,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            Status = request.Status
        };

        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// 更新资源
    /// </summary>
    /// <param name="id">资源标识</param>
    /// <param name="request">更新资源请求</param>
    /// <returns>更新后的资源</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">资源不存在</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceResponse>> Update(Guid id, [FromBody] UpdateResourceRequest request)
    {
        var command = new UpdateResourceCommand
        {
            Id = id,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            HttpMethod = request.HttpMethod,
            Path = request.Path,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            Status = request.Status
        };

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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
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
    [ProducesResponseType(typeof(ResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceResponse>> Activate(Guid id)
    {
        var command = new ActivateResourceCommand { Id = id };
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
    [ProducesResponseType(typeof(ResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceResponse>> Disable(Guid id)
    {
        var command = new DisableResourceCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
