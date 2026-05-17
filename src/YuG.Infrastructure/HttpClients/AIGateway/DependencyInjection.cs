using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace YuG.Infrastructure.HttpClients.AIGateway;

/// <summary>AI.Gateway 客户端依赖注入配置。</summary>
public static class DependencyInjection
{
    /// <summary>注册 <see cref="IChatClient"/> Refit 客户端。</summary>
    /// <param name="services">服务集合</param>
    /// <param name="baseUrl">AI.Gateway 服务地址</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddAiGatewayClient(this IServiceCollection services, string baseUrl)
    {
        services
            .AddRefitClient<IChatClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

        return services;
    }
}
