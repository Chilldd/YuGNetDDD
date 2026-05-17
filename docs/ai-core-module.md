# AI Core 模块设计文档 — YuG.AI.Gateway

## 概述

YuG.AI.Gateway 是独立的 AI 网关微服务，对外提供统一的 HTTP API，对内基于 `Microsoft.Extensions.AI` 统一 AI 客户端抽象和 `SemanticKernel` 编排引擎构建。

**设计原则**：完全独立的微服务，不依赖任何业务模块，可独立部署和扩缩容。

---

## 整体架构

```
┌──────────────────────────────────────────────────────┐
│                 YuG.AI.Gateway                       │
│                      Port 5100                       │
│                                                      │
│  ┌──────────────┐   ┌────────────────────────────┐  │
│  │  Controllers  │   │  Services                  │  │
│  │               │   │                            │  │
│  │ ChatController│──▶│  ChatService               │  │
│  │  /chat/       │   │  ┌─────────────────────┐  │  │
│  │  completions  │   │  │ IChatClient         │  │  │
│  │  + stream     │   │  │ (Microsoft.Extensions│  │  │
│  │               │   │  │  .AI)               │  │  │
│  │ HealthControl │   │  └─────────┬───────────┘  │  │
│  │ ler /health   │   │            │              │  │
│  └──────────────┘   │  ┌─────────▼───────────┐  │  │
│                      │  │ Kernel              │  │  │
│                      │  │ (SemanticKernel)    │  │  │
│                      │  └─────────────────────┘  │  │
│                      └────────────────────────────┘  │
│                                                      │
│  ┌──────────────────────────────────────────────┐   │
│  │  AI Providers                                 │   │
│  │  DeepSeek (OpenAI 兼容) / Azure / Ollama     │   │
│  └──────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────┘
```

---

## 设计目标

- **多 Provider 支持**：通过 `IChatClient` 统一抽象，对接 DeepSeek（OpenAI 兼容协议）、Azure、Ollama 等
- **流式/非流式**：同时支持普通补全和 SSE 流式响应
- **可观测**：记录调用次数、Token 用量、延迟等指标
- **可扩展**：通过 SemanticKernel 预留 plugin、prompt templating 等编排能力
- **容错**：支持超时、重试、降级策略

---

## 项目结构

```
src/YuG.AI.Gateway/
├── Program.cs                                  ← 入口 + 服务注册
├── YuG.AI.Gateway.csproj                       ← M.E.AI + SemanticKernel
├── Controllers/
│   ├── ChatController.cs                       ← POST /api/v1/chat/completions, /chat/stream
│   └── HealthController.cs                     ← GET  /api/v1/health
├── Services/
│   ├── IChatService.cs                         ← 聊天服务接口
│   └── ChatService.cs                          ← 聊天服务实现（调用 IChatClient）
├── Configuration/
│   └── AiOptions.cs                            ← 根配置模型 + 各 Provider 配置
├── Models/
│   ├── Requests/
│   │   ├── ChatRequest.cs                      ← 聊天请求
│   │   └── ChatStreamRequest.cs                ← 流式聊天请求
│   └── Responses/
│       ├── ChatResponse.cs                     ← 聊天响应
│       └── HealthResponse.cs                   ← 健康检查响应
├── Middleware/
│   └── AiExceptionHandlingMiddleware.cs        ← 全局异常处理
├── Extensions/
│   └── ServiceCollectionExtensions.cs          ← AddAiOptions() + AddAiCoreServices()
├── Properties/
│   └── launchSettings.json
└── appsettings.json
```

---

## API 端点

### POST /api/v1/chat/completions

普通对话补全（非流式）。

**请求：**
```json
{
  "messages": [
    { "role": "system", "content": "You are a helpful assistant." },
    { "role": "user", "content": "Hello!" }
  ],
  "temperature": 0.7,
  "maxTokens": 2048
}
```

**响应 200：**
```json
{
  "choices": [
    {
      "index": 0,
      "message": { "role": "assistant", "content": "Hi! How can I help you?" },
      "finishReason": "stop"
    }
  ],
  "model": "deepseek-chat",
  "usage": {
    "promptTokens": 20,
    "completionTokens": 10,
    "totalTokens": 30
  }
}
```

**实现流程：**
1. Controller 验证输入
2. 调用 `IChatService.ChatAsync(request)`
3. Service 映射消息到 `IList<Microsoft.Extensions.AI.ChatMessage>`
4. 调用 `IChatClient.CompleteAsync()` 获取响应
5. 映射为 `ChatResponse` 返回

### POST /api/v1/chat/stream

SSE 流式对话补全。

**请求：** 与 `/chat/completions` 相同

**响应：** `text/event-stream`
```
data: {"type":"delta","content":"Hello"}
data: {"type":"delta","content":"! How"}
data: {"type":"delta","content":" can I"}
data: {"type":"delta","content":" help?"}
data: {"type":"done","id":"chatcmpl-xxx"}
```

**实现流程：**
1. Controller 设置 `Response.ContentType = "text/event-stream"`
2. Service 迭代 `await foreach (var delta in chatClient.CompleteStreamingAsync(...))`
3. 每个 delta 写入 SSE 格式到 response body
4. 每次写入后 flush

### GET /api/v1/health

健康检查。

**响应 200：**
```json
{
  "status": "Healthy",
  "provider": "DeepSeek",
  "model": "deepseek-chat",
  "latencyMs": 342,
  "version": "1.0.0"
}
```

**响应 503：** Provider 不可用时

---

## 核心集成：IChatClient + SemanticKernel

### 注册流程

DeepSeek 使用 OpenAI 兼容协议，通过 `Microsoft.Extensions.AI.OpenAI` 包的 `OpenAIClient` + 自定义 endpoint 对接。

```csharp
// Step 1: 注册 IChatClient（按配置切换 Provider）
services.AddSingleton<IChatClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
    return options.Provider.ToLowerInvariant() switch
    {
        "deepseek" => new OpenAIChatClient(
            new OpenAIClient(options.DeepSeek.ApiKey, new OpenAIClientOptions
            {
                Endpoint = new Uri(options.DeepSeek.BaseUrl)
            }),
            options.DeepSeek.ModelId),

        "azureopenai" => new AzureOpenAIClient(
            new Uri(options.AzureOpenAI.Endpoint),
            new AzureKeyCredential(options.AzureOpenAI.ApiKey))
            .AsChatClient(options.AzureOpenAI.ModelId),

        "ollama" => new OllamaChatClient(
            options.Ollama.Endpoint,
            options.Ollama.ModelId),

        _ => throw new InvalidOperationException($"Unsupported provider: {options.Provider}")
    };
});

// Step 2: 注册 SK Kernel（消费 IChatClient，预留编排能力）
services.AddSingleton<Kernel>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    var builder = Kernel.CreateBuilder();
    builder.AddChatCompletion(chatClient);
    return builder.Build();
});
```

### ChatService 实现

```csharp
public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;

    public ChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct)
    {
        var messages = request.Messages
            .Select(m => new Microsoft.Extensions.AI.ChatMessage(
                new ChatRole(m.Role), m.Content))
            .ToList();

        var response = await _chatClient.CompleteAsync(messages, new()
        {
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
        }, ct);

        return MapResponse(response);
    }

    public async IAsyncEnumerable<ChatStreamDelta> ChatStreamAsync(
        ChatStreamRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        var messages = request.Messages
            .Select(m => new Microsoft.Extensions.AI.ChatMessage(
                new ChatRole(m.Role), m.Content))
            .ToList();

        await foreach (var delta in _chatClient.CompleteStreamingAsync(messages, null, ct))
        {
            if (delta.Contents is { Count: > 0 } contents)
            {
                foreach (var content in contents)
                {
                    if (content is TextContent text)
                        yield return new ChatStreamDelta(text.Text);
                }
            }
        }
    }
}
```

---

## 配置模型

### AiOptions

```csharp
namespace YuG.AI.Gateway.Configuration;

public class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>AI 提供者（deepseek / azureopenai / ollama）</summary>
    public string Provider { get; set; } = "DeepSeek";

    /// <summary>DeepSeek 配置（默认，OpenAI 兼容协议）</summary>
    public DeepSeekConfig DeepSeek { get; set; } = new();

    /// <summary>Azure OpenAI 配置（备选）</summary>
    public AzureOpenAiConfig AzureOpenAI { get; set; } = new();

    /// <summary>Ollama 配置（备选，本地部署）</summary>
    public OllamaConfig Ollama { get; set; } = new();

    /// <summary>默认聊天参数</summary>
    public ChatDefaults Defaults { get; set; } = new();
}

/// <summary>
/// DeepSeek 配置（OpenAI 兼容协议）
/// </summary>
public class DeepSeekConfig
{
    /// <summary>API 密钥</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>API 地址，默认 DeepSeek 官方地址</summary>
    public string BaseUrl { get; set; } = "https://api.deepseek.com/v1";

    /// <summary>模型 ID</summary>
    public string ModelId { get; set; } = "deepseek-chat";
}

public class AzureOpenAiConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gpt-4o";
    public string? DeploymentName { get; set; }
}

public class OllamaConfig
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ModelId { get; set; } = "llama3";
}

public class ChatDefaults
{
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4096;
}
```

### appsettings.json

```json
{
  "Ai": {
    "Provider": "DeepSeek",
    "DeepSeek": {
      "ApiKey": "",
      "BaseUrl": "https://api.deepseek.com/v1",
      "ModelId": "deepseek-chat"
    },
    "AzureOpenAI": {
      "Endpoint": "",
      "ApiKey": "",
      "ModelId": "gpt-4o",
      "DeploymentName": null
    },
    "Ollama": {
      "Endpoint": "http://localhost:11434",
      "ModelId": "llama3"
    },
    "Defaults": {
      "Temperature": 0.7,
      "MaxTokens": 4096
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5100"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.SemanticKernel": "Warning",
      "Azure": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 设计决策

| 决策 | 选择 | 理由 |
|------|------|------|
| **默认 Provider** | DeepSeek（OpenAI 兼容协议） | 用户指定对接 deepseekv4-flash |
| **AI 抽象层** | Microsoft.Extensions.AI.IChatClient | .NET 官方统一抽象，DeepSeek 通过 OpenAI 兼容包对接 |
| **编排引擎** | SemanticKernel（预留） | 后续可扩展 plugin、prompt template 等高级能力 |
| **通信协议** | HTTP/JSON + SSE | 通用、无额外中间件依赖 |
| **流式支持** | SSE（Server-Sent Events） | 实现简单，前端兼容性好 |
| **IChatClient 生命周期** | Singleton | 线程安全，避免反复构造 |
| **Provider 切换** | 启动时静态选择 | 运行时切换复杂度高 |
| **对话状态** | 无状态 | 每次请求独立，不维护对话历史 |

---

## NuGet 包依赖

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="10.4.1" />
    <PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="10.4.1" />
    <PackageReference Include="Microsoft.Extensions.AI.AzureAIInference" Version="10.4.1" />
    <PackageReference Include="Microsoft.Extensions.AI.Ollama" Version="10.4.1" />
    <PackageReference Include="Microsoft.SemanticKernel" Version="1.45.0" />
</ItemGroup>
```
