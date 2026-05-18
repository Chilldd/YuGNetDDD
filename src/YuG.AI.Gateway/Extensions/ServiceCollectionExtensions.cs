using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Middleware;
using YuG.AI.Gateway.Plugins;
using YuG.AI.Gateway.Services;

namespace YuG.AI.Gateway.Extensions;

/// <summary>AI 网关服务注册扩展方法。</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>注册 AI 配置选项。</summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">应用程序配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAiOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        return services;
    }

    /// <summary>注册 AI 插件。</summary>
    /// <param name="services">服务集合</param>
    /// <typeparam name="T">插件类型，需实现 <see cref="IAiPlugin"/></typeparam>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAiPlugin<T>(this IServiceCollection services)
        where T : class, IAiPlugin
        => services.AddSingleton<IAiPlugin, T>();

    /// <summary>注册 AI 核心服务（Kernel、IChatService）。</summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAiCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<Kernel>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            var builder = Kernel.CreateBuilder();

            switch (options.Provider.ToLowerInvariant())
            {
                case "deepseek":
                {
                    if (options.DeepSeek.DisableThinking)
                    {
                        var innerHandler = new HttpClientHandler();
                        var thinkingHandler = new DisableThinkingHandler { InnerHandler = innerHandler };
                        var httpClient = new HttpClient(thinkingHandler);
                        builder.AddOpenAIChatCompletion(
                            options.DeepSeek.ModelId,
                            new Uri(options.DeepSeek.BaseUrl),
                            options.DeepSeek.ApiKey,
                            httpClient: httpClient);
                    }
                    else
                    {
                        builder.AddOpenAIChatCompletion(
                            options.DeepSeek.ModelId,
                            new Uri(options.DeepSeek.BaseUrl),
                            options.DeepSeek.ApiKey);
                    }
                    break;
                }
                default:
                    throw new InvalidOperationException($"Unsupported AI provider: {options.Provider}");
            }

            // 注册所有插件
            foreach (var plugin in sp.GetServices<IAiPlugin>())
            {
                builder.Plugins.AddFromObject(plugin, plugin.Name);
            }

            return builder.Build();
        });

        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}
