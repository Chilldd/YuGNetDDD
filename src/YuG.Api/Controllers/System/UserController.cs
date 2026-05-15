using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Helpers;
using YuG.Application.Identity.Role.SetUserRoles;
using YuG.Application.Identity.User.Activate;
using YuG.Application.Identity.User.Create;
using YuG.Application.Identity.User.Delete;
using YuG.Application.Identity.User.Disable;
using YuG.Application.Identity.User.Get;
using YuG.Application.Identity.User.GetList;
using YuG.Application.Identity.User.RemoveRole;
using YuG.Application.Identity.User.ResetPassword;

namespace YuG.Api.Controllers.System;

/// <summary>
/// 用户管理控制器
/// </summary>
[ApiController]
[Authorize]
[Route("api/system/user")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化用户管理控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <param name="query">获取用户列表查询</param>
    /// <returns>用户列表</returns>
    /// <response code="200">查询成功</response>
    [HttpGet]
    [ApiDescription("获取用户列表")]
    [Authorize(Policy = "user:get")]
    [ProducesResponseType(typeof(GetUserListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetUserListResult>> GetList([FromQuery] GetUserListQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// 获取单个用户
    /// </summary>
    /// <param name="id">用户标识</param>
    /// <returns>用户详细信息</returns>
    /// <response code="200">查询成功</response>
    /// <response code="404">用户不存在</response>
    [HttpGet("{id}")]
    [ApiDescription("获取单个用户")]
    [Authorize(Policy = "user:getbyid")]
    [ProducesResponseType(typeof(GetUserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetUserResult>> GetById(long id)
    {
        var query = new GetUserQuery { Id = id };
        var response = await _mediator.Send(query);
        if (response is null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="command">创建用户命令</param>
    /// <returns>创建的用户</returns>
    /// <response code="201">创建成功</response>
    /// <response code="400">请求参数无效或用户名已存在</response>
    [HttpPost]
    [ApiDescription("创建用户")]
    [Authorize(Policy = "user:create")]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResult>> Create([FromBody] CreateUserCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(null, new { id = response.Id }, response);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户标识</param>
    /// <returns>删除结果</returns>
    /// <response code="204">删除成功</response>
    /// <response code="404">用户不存在</response>
    [HttpDelete("{id}")]
    [ApiDescription("删除用户")]
    [Authorize(Policy = "user:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteUserCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// 启用用户
    /// </summary>
    /// <param name="id">用户标识</param>
    /// <returns>启用后的用户</returns>
    /// <response code="200">启用成功</response>
    /// <response code="404">用户不存在</response>
    [HttpPost("{id}/activate")]
    [ApiDescription("启用用户")]
    [Authorize(Policy = "user:activate")]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResult>> Activate(long id)
    {
        var command = new ActivateUserCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 禁用用户
    /// </summary>
    /// <param name="id">用户标识</param>
    /// <returns>禁用后的用户</returns>
    /// <response code="200">禁用成功</response>
    /// <response code="404">用户不存在</response>
    [HttpPost("{id}/disable")]
    [ApiDescription("禁用用户")]
    [Authorize(Policy = "user:disable")]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResult>> Disable(long id)
    {
        var command = new DisableUserCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    /// <param name="id">用户标识</param>
    /// <returns>重置密码后的用户</returns>
    /// <response code="200">重置成功</response>
    /// <response code="404">用户不存在</response>
    [HttpPost("{id}/reset-password")]
    [ApiDescription("重置用户密码")]
    [Authorize(Policy = "user:resetpassword")]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResult>> ResetPassword(long id)
    {
        var command = new ResetPasswordCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 设置用户角色（覆盖模式：删除旧角色，保存新角色）
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <param name="command">设置用户角色命令</param>
    /// <returns>操作结果</returns>
    /// <response code="204">设置成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">用户或角色不存在</response>
    [HttpPut("{userId}/roles")]
    [ApiDescription("设置用户角色")]
    [Authorize(Policy = "user:setroles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRoles(long userId, [FromBody] SetUserRolesCommand command)
    {
        if (userId != command.UserId)
        {
            return BadRequest("用户标识不匹配");
        }

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// 移除用户角色（批量：从多个用户移除同一个角色）
    /// </summary>
    /// <param name="roleId">角色标识</param>
    /// <param name="command">移除用户角色命令</param>
    /// <returns>操作结果</returns>
    /// <response code="204">移除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">用户不存在</response>
    [HttpDelete("roles/{roleId}")]
    [ApiDescription("移除用户角色")]
    [Authorize(Policy = "user:removerole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(long roleId, [FromBody] RemoveUserRoleCommand command)
    {
        var cmd = new RemoveUserRoleCommand { RoleId = roleId, UserIds = command.UserIds };
        await _mediator.Send(cmd);
        return NoContent();
    }
}
