using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuG.Api.Helpers;
using YuG.Application.Common.Interfaces;

namespace YuG.Api.Controllers;

/// <summary>
/// 测试控制器（需要认证）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly IUserIdentity _userIdentity;

    /// <summary>
    /// 初始化测试控制器
    /// </summary>
    /// <param name="userIdentity">当前用户身份信息</param>
    public WeatherController(IUserIdentity userIdentity)
    {
        _userIdentity = userIdentity;
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>当前用户信息</returns>
    /// <response code="200">成功返回用户信息</response>
    /// <response code="401">未授权</response>
    [HttpGet("me")]
    [ApiDescription("获取当前用户信息")]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        return Ok(new UserInfo
        {
            UserId = _userIdentity.UserId,
            Username = _userIdentity.Username
        });
    }
}

/// <summary>
/// 用户信息
/// </summary>
public record UserInfo
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;
}
