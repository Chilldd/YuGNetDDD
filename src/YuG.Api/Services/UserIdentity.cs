using System.Collections.Immutable;
using System.Security.Claims;
using YuG.Application.Common.Interfaces;

namespace YuG.Api.Services;

/// <summary>
/// 当前用户身份信息（从 JWT Claims 中提取）
/// </summary>
public class UserIdentity : IUserIdentity
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private long? _cachedUserId;
    private string? _cachedUsername;
    private IReadOnlyList<string>? _cachedRoles;
    private bool _userIdResolved;
    private bool _usernameResolved;
    private bool _rolesResolved;

    /// <summary>
    /// 初始化当前用户身份信息
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器</param>
    public UserIdentity(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 当前用户标识
    /// </summary>
    public long UserId
    {
        get
        {
            if (!_userIdResolved)
            {
                _cachedUserId = ResolveUserId();
                _userIdResolved = true;
            }
            return _cachedUserId!.Value;
        }
    }

    /// <summary>
    /// 当前用户名
    /// </summary>
    public string Username
    {
        get
        {
            if (!_usernameResolved)
            {
                _cachedUsername = ResolveUsername();
                _usernameResolved = true;
            }
            return _cachedUsername!;
        }
    }

    /// <summary>
    /// 当前用户的角色编码列表
    /// </summary>
    public IReadOnlyList<string> Roles
    {
        get
        {
            if (!_rolesResolved)
            {
                _cachedRoles = ResolveRoles();
                _rolesResolved = true;
            }
            return _cachedRoles!;
        }
    }

    private long ResolveUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(value) || !long.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("无法从令牌中解析用户标识");
        }
        return userId;
    }

    private string ResolveUsername()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value
            ?? throw new UnauthorizedAccessException("无法从令牌中解析用户名");
    }

    private IReadOnlyList<string> ResolveRoles()
    {
        var roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct()
            .ToImmutableList();

        return roles ?? [];
    }
}
