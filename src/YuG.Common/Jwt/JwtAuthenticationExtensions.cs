using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YuG.Common.Interfaces;

namespace YuG.Common.Jwt;

/// <summary>
/// JWT 认证服务注册扩展方法
/// </summary>
public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// 注册 JWT Bearer 认证，包含令牌世代版本校验
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // 绑定 JWT 配置
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddAuthentication("Bearer")
            .AddJwtBearer();

        // 配置 JwtBearerOptions，从 DI 解析 JwtOptions
        services.AddOptions<JwtBearerOptions>("Bearer")
            .Configure<IOptions<JwtOptions>>((bearerOpts, jwtOptions) =>
            {
                var validator = new JwtTokenValidator(jwtOptions.Value);
                bearerOpts.TokenValidationParameters = validator.CreateTokenValidationParameters();
                bearerOpts.Events = new JwtBearerEvents
                {
                    OnTokenValidated = OnTokenValidated
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static async Task OnTokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal;
        if (principal is null)
        {
            context.Fail("令牌无效");
            return;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var genClaim = principal.FindFirst("gen")?.Value;

        if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
        {
            context.Fail("令牌中缺少用户标识");
            return;
        }

        var cache = context.HttpContext.RequestServices.GetRequiredService<ICache>();
        var cachedGenStr = await cache.GetAsync<string>($"token_gen:{userId}");

        if (cachedGenStr is not null && int.TryParse(cachedGenStr, out var cachedGen))
        {
            var tokenGen = genClaim is not null && int.TryParse(genClaim, out var g) ? g : 0;
            if (tokenGen != cachedGen)
            {
                context.Fail("令牌已失效，请重新登录");
                return;
            }
        }
    }
}
