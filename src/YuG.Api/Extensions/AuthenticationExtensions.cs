using Microsoft.AspNetCore.Authorization;
using YuG.Api.Authorization;

namespace YuG.Api.Extensions;

/// <summary>
/// API 层授权配置扩展方法
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// 注册 API 层授权策略和权限鉴权处理器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();

        // 注册权限编码鉴权处理器（Scoped，单次请求内可缓存权限列表）
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // 注册动态策略提供者，支持 [RequirePermission("code")] 自动创建策略
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }
}
