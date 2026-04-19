using MediatR;
using Microsoft.AspNetCore.Mvc;
using YuG.Application.Identity.UserLogin.Login;
using YuG.Application.Identity.UserLogin.Logout;
using YuG.Application.Identity.UserLogin.RefreshToken;

namespace YuG.Api.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化认证控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="command">登录命令</param>
    /// <returns>登录结果，包含访问令牌和刷新令牌</returns>
    /// <response code="200">登录成功</response>
    /// <response code="401">用户名或密码错误</response>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="command">刷新令牌命令</param>
    /// <returns>新的访问令牌和刷新令牌</returns>
    /// <response code="200">刷新成功</response>
    /// <response code="401">刷新令牌无效或已过期</response>
    [HttpPost]
    [Route("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResult>> Refresh([FromBody] RefreshTokenCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <param name="command">登出命令</param>
    /// <returns>登出结果</returns>
    /// <response code="204">登出成功</response>
    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
