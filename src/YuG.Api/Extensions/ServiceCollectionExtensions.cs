using FluentValidation;
using MediatR;
using YuG.Api.Helpers;
using YuG.Api.Services;
using YuG.Application.Common;
using YuG.Application.Common.Behaviors;
using YuG.Application.Common.Interfaces;

namespace YuG.Api.Extensions;

/// <summary>
/// 服务注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册应用程序层服务（MediatR、FluentValidation）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 注册 MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CommandBase<>).Assembly);
        });

        // 注册 FluentValidation
        services.AddValidatorsFromAssembly(typeof(CommandBase<>).Assembly);

        // 注册 MediatR 管道行为
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }

    /// <summary>
    /// 注册 API 层工具服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddApiTools(this IServiceCollection services)
    {
        // 注册 API 端点扫描器
        services.AddScoped<IApiEndpointScanner, ApiEndpointScanner>();

        // 注册当前用户身份信息（Scoped，每个请求解析一次）
        services.AddScoped<IUserIdentity, UserIdentity>();

        return services;
    }

    /// <summary>
    /// 添加 CORS 策略配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">应用配置</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection("Cors");
        var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", policy =>
            {
                if (allowedOrigins.Length != 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }
}
