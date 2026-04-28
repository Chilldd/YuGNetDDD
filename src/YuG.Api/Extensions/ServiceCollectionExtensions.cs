using FluentValidation;
using MediatR;
using YuG.Api.Helpers;
using YuG.Application.Common;
using YuG.Application.Common.Behaviors;

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
        return services;
    }
}
