namespace YuG.AI.Gateway.Models.Responses;

/// <summary>健康检查响应。</summary>
public class HealthResponse
{
    /// <summary>健康状态（Healthy / Unhealthy）。</summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>AI Provider 名称。</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>当前使用的模型标识。</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>请求延迟（毫秒）。</summary>
    public long LatencyMs { get; set; }

    /// <summary>网关版本。</summary>
    public string Version { get; set; } = "1.0.0";
}
