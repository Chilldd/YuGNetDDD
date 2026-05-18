namespace YuG.AI.Gateway.Configuration;

/// <summary>AI 网关配置选项。</summary>
public class AiOptions
{
    /// <summary>配置文件节名称。</summary>
    public const string SectionName = "Ai";

    /// <summary>AI Provider 名称（deepseek / azureopenai / ollama）。</summary>
    public string Provider { get; set; } = "DeepSeek";

    /// <summary>系统提示词，创建新会话时自动注入。</summary>
    public string? SystemPrompt { get; set; }

    /// <summary>DeepSeek 配置。</summary>
    public DeepSeekConfig DeepSeek { get; set; } = new();

    /// <summary>Azure OpenAI 配置。</summary>
    public AzureOpenAiConfig AzureOpenAI { get; set; } = new();

    /// <summary>Ollama 配置。</summary>
    public OllamaConfig Ollama { get; set; } = new();

    /// <summary>工具调用最大轮数（防止无限循环，默认 10）。</summary>
    public int MaxToolCallRounds { get; set; } = 10;

    /// <summary>启动时校验配置是否有效。</summary>
    /// <exception cref="InvalidOperationException">配置不完整时抛出</exception>
    public void Validate()
    {
        switch (Provider.ToLowerInvariant())
        {
            case "deepseek":
                if (string.IsNullOrWhiteSpace(DeepSeek.ApiKey))
                    throw new InvalidOperationException("DeepSeek ApiKey 未配置");
                if (string.IsNullOrWhiteSpace(DeepSeek.BaseUrl))
                    throw new InvalidOperationException("DeepSeek BaseUrl 未配置");
                break;
            default:
                throw new InvalidOperationException($"不支持的 AI Provider: {Provider}");
        }
    }
}

/// <summary>DeepSeek 配置。</summary>
public class DeepSeekConfig
{
    /// <summary>API 密钥。</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>API 基础地址。</summary>
    public string BaseUrl { get; set; } = "https://api.deepseek.com";

    /// <summary>模型标识。</summary>
    public string ModelId { get; set; } = "deepseek-v4-flash";
}

/// <summary>Azure OpenAI 配置。</summary>
public class AzureOpenAiConfig
{
    /// <summary>终结点地址。</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>API 密钥。</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>模型标识。</summary>
    public string ModelId { get; set; } = "gpt-4o";

    /// <summary>部署名称，通常与 ModelId 相同。</summary>
    public string? DeploymentName { get; set; }
}

/// <summary>Ollama 配置。</summary>
public class OllamaConfig
{
    /// <summary>Ollama 服务地址。</summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>模型标识。</summary>
    public string ModelId { get; set; } = "llama3";
}

