using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using Azure.AI.OpenAI;
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

    /// <summary>注册 AI 核心服务（IChatClient、IChatService）。</summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAiCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            return options.Provider.ToLowerInvariant() switch
            {
                "deepseek" => new OpenAIClient(
                    new ApiKeyCredential(options.DeepSeek.ApiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(options.DeepSeek.BaseUrl) })
                    .GetChatClient(options.DeepSeek.ModelId)
                    .AsIChatClient(),

                "azureopenai" => new AzureOpenAIClient(
                    new Uri(options.AzureOpenAI.Endpoint),
                    new ApiKeyCredential(options.AzureOpenAI.ApiKey))
                    .GetChatClient(options.AzureOpenAI.ModelId)
                    .AsIChatClient(),

                "ollama" => new OpenAIClient(
                    new ApiKeyCredential("ollama"),
                    new OpenAIClientOptions { Endpoint = new Uri(options.Ollama.Endpoint) })
                    .GetChatClient(options.Ollama.ModelId)
                    .AsIChatClient(),

                _ => throw new InvalidOperationException($"Unsupported AI provider: {options.Provider}")
            };
        });

        services.AddSingleton<ISessionService, SessionService>();
        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}
