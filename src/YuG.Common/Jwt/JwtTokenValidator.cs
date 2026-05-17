using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace YuG.Common.Jwt;

/// <summary>
/// JWT 令牌校验器，封装 TokenValidationParameters 创建和令牌验证逻辑
/// </summary>
public class JwtTokenValidator
{
    private readonly JwtOptions _options;

    /// <summary>
    /// 初始化 JWT 令牌校验器
    /// </summary>
    /// <param name="options">JWT 配置选项</param>
    public JwtTokenValidator(JwtOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 创建令牌验证参数
    /// </summary>
    public TokenValidationParameters CreateTokenValidationParameters()
    {
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    /// <summary>
    /// 验证令牌并返回 ClaimsPrincipal
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>验证通过返回 ClaimsPrincipal，失败返回 null</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = CreateTokenValidationParameters();

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从令牌中提取用户 ID（不做签名验证）
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>用户 ID，提取失败返回 null</returns>
    public long? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (subClaim is not null && long.TryParse(subClaim.Value, out var userId))
                return userId;

            return null;
        }
        catch
        {
            return null;
        }
    }
}
