using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Application.Identity.Role.Activate;
using YuG.Application.Identity.Role.Create;
using YuG.Application.Identity.Role.Delete;
using YuG.Application.Identity.Role.Disable;
using YuG.Application.Identity.Role.Get;
using YuG.Application.Identity.Role.GetList;
using YuG.Application.Identity.Role.AssignResource;
using YuG.Application.Identity.Role.UnassignResource;
using CreateRoleCommands = YuG.Application.Identity.Role.Create;
using UpdateRoleCommands = YuG.Application.Identity.Role.Update;

namespace YuG.Api.Controllers;

/// <summary>
/// 角色管理控制器
/// </summary>
[ApiController]
[Authorize]
[Route("api/role")]
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化角色管理控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    public RoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <param name="query">获取角色列表查询</param>
    /// <returns>角色列表</returns>
    /// <response code="200">查询成功</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetRoleListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetRoleListResult>> Get([FromQuery] GetRoleListQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// 获取单个角色
    /// </summary>
    /// <param name="id">角色标识</param>
    /// <returns>角色详细信息</returns>
    /// <response code="200">查询成功</response>
    /// <response code="404">角色不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetRoleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetRoleResult>> GetById(long id)
    {
        var query = new GetRoleQuery { Id = id };
        var response = await _mediator.Send(query);
        if (response is null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="command">创建角色命令</param>
    /// <returns>创建的角色</returns>
    /// <response code="201">创建成功</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRoleCommands.RoleResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateRoleCommands.RoleResult>> Create([FromBody] CreateRoleCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id">角色标识</param>
    /// <param name="command">更新角色命令</param>
    /// <returns>更新后的角色</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">角色不存在</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CreateRoleCommands.RoleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateRoleCommands.RoleResult>> Update(long id, [FromBody] UpdateRoleCommands.UpdateRoleCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("角色标识不匹配");
        }

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id">角色标识</param>
    /// <returns>删除结果</returns>
    /// <response code="204">删除成功</response>
    /// <response code="404">角色不存在</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteRoleCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// 激活角色
    /// </summary>
    /// <param name="id">角色标识</param>
    /// <returns>激活后的角色</returns>
    /// <response code="200">激活成功</response>
    /// <response code="404">角色不存在</response>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(CreateRoleCommands.RoleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateRoleCommands.RoleResult>> Activate(long id)
    {
        var command = new ActivateRoleCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 禁用角色
    /// </summary>
    /// <param name="id">角色标识</param>
    /// <returns>禁用后的角色</returns>
    /// <response code="200">禁用成功</response>
    /// <response code="404">角色不存在</response>
    [HttpPost("{id}/disable")]
    [ProducesResponseType(typeof(CreateRoleCommands.RoleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateRoleCommands.RoleResult>> Disable(long id)
    {
        var command = new DisableRoleCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 给角色分配资源（覆盖模式）
    /// </summary>
    /// <param name="id">角色标识</param>
    /// <param name="command">分配资源命令</param>
    /// <returns>分配后的角色</returns>
    /// <response code="200">分配成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">角色不存在</response>
    [HttpPost("{id}/resources")]
    [ProducesResponseType(typeof(CreateRoleCommands.RoleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateRoleCommands.RoleResult>> AssignResources(long id, [FromBody] AssignResourceCommand command)
    {
        if (id != command.RoleId)
        {
            return BadRequest("角色标识不匹配");
        }

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 从角色移除资源
    /// </summary>
    /// <param name="roleId">角色标识</param>
    /// <param name="resourceId">资源标识</param>
    /// <returns>移除结果</returns>
    /// <response code="204">移除成功</response>
    /// <response code="404">角色或资源不存在</response>
    [HttpDelete("{roleId}/resources/{resourceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignResource(long roleId, long resourceId)
    {
        var command = new UnassignResourceCommand { RoleId = roleId, ResourceId = resourceId };
        await _mediator.Send(command);
        return NoContent();
    }
}
