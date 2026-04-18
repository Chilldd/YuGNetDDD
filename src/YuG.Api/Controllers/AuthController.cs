using MediatR;
using Microsoft.AspNetCore.Mvc;
using YuG.Application.Commands.Auth.Login;
using YuG.Application.Commands.Auth.Logout;
using YuG.Application.Commands.Auth.RefreshToken;
using YuG.Application.DTOs.Requests;
using YuG.Application.DTOs.Responses;

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
    /// <param name="request">登录请求</param>
    /// <returns>登录结果，包含访问令牌和刷新令牌</returns>
    /// <response code="200">登录成功</response>
    /// <response code="401">用户名或密码错误</response>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="request">刷新令牌请求</param>
    /// <returns>新的访问令牌和刷新令牌</returns>
    /// <response code="200">刷新成功</response>
    /// <response code="401">刷新令牌无效或已过期</response>
    [HttpPost]
    [Route("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <param name="request">登出请求</param>
    /// <returns>登出结果</returns>
    /// <response code="204">登出成功</response>
    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand
        {
            RefreshToken = request.RefreshToken
        };

        await _mediator.Send(command);
        return NoContent();
    }
}
