using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Application.Identity.UserRole.SetUserRoles;

namespace YuG.Api.Controllers;

/// <summary>
/// 用户管理控制器
/// </summary>
[ApiController]
[Authorize]
[Route("api/user")]
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
    /// 设置用户角色（覆盖模式：删除旧角色，保存新角色）
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <param name="command">设置用户角色命令</param>
    /// <returns>操作结果</returns>
    /// <response code="204">设置成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">用户或角色不存在</response>
    [HttpPut("{userId}/roles")]
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
}
