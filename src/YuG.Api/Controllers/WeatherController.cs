using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YuG.Api.Controllers;

/// <summary>
/// 测试控制器（需要认证）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>当前用户信息</returns>
    /// <response code="200">成功返回用户信息</response>
    /// <response code="401">未授权</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        return Ok(new UserInfo
        {
            UserId = userId ?? string.Empty,
            Username = username ?? string.Empty
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
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;
}
