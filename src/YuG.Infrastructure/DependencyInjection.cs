using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YuG.Application.Interfaces;
using YuG.Domain.Interfaces;
using YuG.Domain.Repositories;
using YuG.Infrastructure.Persistence;
using YuG.Infrastructure.Repositories;
using YuG.Infrastructure.Services;

namespace YuG.Infrastructure;

/// <summary>
/// 基础设施层依赖注入配置
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 注册基础设施层所有服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册数据库上下文
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // 注册数据库上下文接口
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // 注册数据库初始化器
        services.AddScoped<ApplicationDbContextInitializer>();

        // 注册认证服务
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // 注册 JWT 令牌服务
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // 注册密码哈希服务
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // 注册用户仓储
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
