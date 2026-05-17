using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YuG.Common.Interfaces;
using YuG.Common.Jwt;

namespace YuG.Infrastructure.Services;

/// <summary>
/// JWT 令牌服务实现
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    /// <summary>
    /// 初始化 JWT 令牌服务
    /// </summary>
    /// <param name="options">JWT 配置选项</param>
    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 生成 JWT 访问令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="username">用户名</param>
    /// <param name="roles">用户角色编码列表</param>
    /// <param name="generation">令牌世代版本</param>
    /// <returns>JWT 访问令牌</returns>
    public string GenerateAccessToken(long userId, string username, IReadOnlyList<string> roles, int generation = 0)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("gen", generation.ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _options.Issuer,
            Audience = _options.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <param name="expirationDays">过期天数</param>
    /// <returns>刷新令牌值</returns>
    public string GenerateRefreshToken(int expirationDays = 7)
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 从 JWT 令牌中提取用户ID
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>用户ID</returns>
    public long? GetUserIdFromToken(string token)
    {
        var validator = new JwtTokenValidator(_options);
        return validator.GetUserIdFromToken(token);
    }

    /// <summary>
    /// 验证令牌是否有效
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>令牌是否有效</returns>
    public bool ValidateToken(string token)
    {
        try
        {
            var validator = new JwtTokenValidator(_options);
            return validator.ValidateToken(token) is not null;
        }
        catch
        {
            return false;
        }
    }
}
