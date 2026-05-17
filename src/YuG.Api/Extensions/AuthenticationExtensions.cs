using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using YuG.Api.Authorization;
using YuG.Common.Interfaces;

namespace YuG.Api.Extensions;

/// <summary>
/// 认证服务扩展方法
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// 添加 JWT 认证服务和权限鉴权
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? string.Empty))
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
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

                        // 从缓存中获取当前令牌世代版本
                        var cache = context.HttpContext.RequestServices.GetRequiredService<ICache>();
                        var cachedGenStr = await cache.GetAsync<string>($"token_gen:{userId}");

                        if (cachedGenStr is not null && int.TryParse(cachedGenStr, out var cachedGen))
                        {
                            // 如果缓存中有记录，比对世代版本
                            var tokenGen = genClaim is not null && int.TryParse(genClaim, out var g) ? g : 0;
                            if (tokenGen != cachedGen)
                            {
                                context.Fail("令牌已失效，请重新登录");
                                return;
                            }
                        }
                        // 缓存中没有记录（如缓存重启），放行——让用户重新登录后会自动写入缓存
                    }
                };
            });

        services.AddAuthorization();

        // 注册权限编码鉴权处理器（Scoped，单次请求内可缓存权限列表）
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // 注册动态策略提供者，支持 [RequirePermission("code")] 自动创建策略
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }
}
