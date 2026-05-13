using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using YuG.Api.Authorization;

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
            });

        services.AddAuthorization();

        // 注册权限编码鉴权处理器（Scoped，单次请求内可缓存权限列表）
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // 注册动态策略提供者，支持 [RequirePermission("code")] 自动创建策略
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }
}
