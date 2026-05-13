using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Helpers;
using YuG.Application.Common.Interfaces;
using YuG.Application.Permission.UserPermission.GetPageApiPermissions;
using YuG.Application.Permission.UserPermission.GetUserMenu;

namespace YuG.Api.Controllers;

/// <summary>
/// 用户权限查询控制器
/// </summary>
[ApiController]
[Authorize]
[Route("api/permission")]
public class PermissionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserIdentity _userIdentity;

    /// <summary>
    /// 初始化用户权限查询控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    /// <param name="userIdentity">当前用户身份信息</param>
    public PermissionController(IMediator mediator, IUserIdentity userIdentity)
    {
        _mediator = mediator;
        _userIdentity = userIdentity;
    }

    /// <summary>
    /// 获取当前用户的菜单树（基于角色拥有的资源）
    /// </summary>
    /// <returns>用户菜单树</returns>
    /// <response code="200">查询成功</response>
    /// <response code="401">未授权</response>
    [HttpGet("menus")]
    [ApiDescription("获取当前用户的菜单树")]
    [ProducesResponseType(typeof(GetUserMenuResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetUserMenuResult>> GetUserMenus()
    {
        var query = new GetUserMenuQuery { UserId = _userIdentity.UserId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定页面的 API 权限编码列表
    /// </summary>
    /// <param name="pageId">页面资源标识</param>
    /// <returns>API 权限编码列表</returns>
    /// <response code="200">查询成功</response>
    /// <response code="401">未授权</response>
    [HttpGet("pages/{pageId}/apis")]
    [ApiDescription("获取页面的 API 权限")]
    [ProducesResponseType(typeof(GetPageApiPermissionsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetPageApiPermissionsResult>> GetPageApiPermissions(long pageId)
    {
        var query = new GetPageApiPermissionsQuery { UserId = _userIdentity.UserId, PageId = pageId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
