using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Models.Responses;

namespace YuG.AI.Gateway.Controllers;

/// <summary>健康检查接口控制器。</summary>
[ApiController]
[Route("api/v1")]
public class HealthController : ControllerBase
{
    private readonly AiOptions _options;
    private readonly ILogger<HealthController> _logger;

    /// <summary>初始化 <see cref="HealthController"/> 实例。</summary>
    /// <param name="options">AI 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public HealthController(IOptions<AiOptions> options, ILogger<HealthController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>健康检查，验证 AI Provider 是否可用。</summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>健康状态响应</returns>
    [HttpGet("health")]
    public async Task<ActionResult<HealthResponse>> Health(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var provider = _options.Provider.ToLowerInvariant();
            var healthUrl = provider switch
            {
                "deepseek" => $"{_options.DeepSeek.BaseUrl.TrimEnd('/')}/models",
                _ => null
            };

            if (healthUrl is not null)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, healthUrl);
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_options.DeepSeek.ApiKey}");
                var response = await httpClient.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();
            }

            sw.Stop();

            return Ok(new HealthResponse
            {
                Status = "Healthy",
                Provider = _options.Provider,
                Model = _options.Provider.ToLowerInvariant() switch
                {
                    "deepseek" => _options.DeepSeek.ModelId,
                    "azureopenai" => _options.AzureOpenAI.ModelId,
                    "ollama" => _options.Ollama.ModelId,
                    _ => "unknown"
                },
                LatencyMs = sw.ElapsedMilliseconds,
                Version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for provider {Provider}", _options.Provider);
            sw.Stop();

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new HealthResponse
            {
                Status = "Unhealthy",
                Provider = _options.Provider,
                Model = _options.Provider.ToLowerInvariant() switch
                {
                    "deepseek" => _options.DeepSeek.ModelId,
                    "azureopenai" => _options.AzureOpenAI.ModelId,
                    "ollama" => _options.Ollama.ModelId,
                    _ => "unknown"
                },
                LatencyMs = sw.ElapsedMilliseconds,
                Version = "1.0.0"
            });
        }
    }
}
