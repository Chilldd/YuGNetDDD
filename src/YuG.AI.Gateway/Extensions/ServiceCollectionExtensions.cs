using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using YuG.AI.Gateway.Configuration;
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

    /// <summary>注册 AI 核心服务（Kernel、ISessionService、IChatService）。</summary>
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
                    builder.AddOpenAIChatCompletion(
                        options.DeepSeek.ModelId,
                        new Uri(options.DeepSeek.BaseUrl),
                        options.DeepSeek.ApiKey);
                    break;

                case "azureopenai":
                    builder.AddAzureOpenAIChatCompletion(
                        options.AzureOpenAI.DeploymentName ?? options.AzureOpenAI.ModelId,
                        options.AzureOpenAI.Endpoint,
                        options.AzureOpenAI.ApiKey,
                        modelId: options.AzureOpenAI.ModelId);
                    break;

                case "ollama":
                    builder.AddOpenAIChatCompletion(
                        options.Ollama.ModelId,
                        new Uri(options.Ollama.Endpoint),
                        "ollama");
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported AI provider: {options.Provider}");
            }

            return builder.Build();
        });

        services.AddSingleton<ISessionService, SessionService>();
        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}
