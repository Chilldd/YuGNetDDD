# 流程图绘制模块设计文档

## 概述

在系统中新增「流程图绘制」模块，用户可以通过项目（分组）管理多个流程图。每个流程图支持两种编辑方式：

1. **AI 对话生成**：用户通过对话描述需求，AI 返回 Mermaid.js DSL 源码，前端渲染为可视化流程图
2. **手动微调**：用户直接在 Mermaid 源码上修改，精确控制流程图细节

用户也可以继续与 AI 对话，在已有流程图基础上进行迭代优化。

AI 对接能力由独立的 **[AI Core 模块](ai-core-module.md)** 提供，不绑定在 FlowDesign 模块中。后续其他模块（如报表生成、文档撰写等）可以复用同一套 AI 基础设施。

### 模块依赖关系

```
FlowDesign 模块                    AI Core 模块
  ┌──────────────────┐    调用     ┌──────────────────┐
  │ SendMessage      │───────────▶│ IAiService       │
  │ Handler          │            │ (Provider 路由)   │
  │ (Mermaid 解析)   │            └──────────────────┘
  └──────────────────┘
```

FlowDesign 通过 `IAiService` 接口调用 AI 能力，具体 AI Core 的设计详见 [ai-core-module.md](ai-core-module.md)。

## 领域模型

### 聚合：FlowProject（流程项目）
- **统一入口**：所有模块通过 `IAiService` 接口调用 AI 能力
- **可扩展响应解析**：每个业务模块注册自己的响应解析器，从 AI 输出中提取结构化数据
- **可观测**：记录调用次数、Token 用量、延迟等指标
- **容错**：支持超时、重试、降级策略

## 领域层定义

### 核心接口

```csharp
// YuG.Domain/Common/Interfaces/IAiService.cs

namespace YuG.Domain.Common.Interfaces;

/// <summary>
/// 统一 AI 服务入口，所有模块通过此接口调用 AI 能力
/// </summary>
public interface IAiService
{
    /// <summary>
    /// 发送 AI 对话请求
    /// </summary>
    /// <param name="request">对话请求参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>AI 响应</returns>
    Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// AI 对话请求
/// </summary>
public record AiCompletionRequest(
    /// <summary>消息列表（按时间正序）</summary>
    IReadOnlyList<AiMessage> Messages,

    /// <summary>系统提示词</summary>
    string? SystemPrompt = null,

    /// <summary>使用的模型标识，null 表示使用默认模型</summary>
    string? Model = null,

    /// <summary>温度参数</summary>
    double? Temperature = null,

    /// <summary>最大 Token 数</summary>
    int? MaxTokens = null
);

/// <summary>
/// AI 对话响应
/// </summary>
public record AiCompletionResponse(
    /// <summary>AI 回复内容</summary>
    string Content,

    /// <summary>实际使用的模型</summary>
    string? Model = null,

    /// <summary>提示 Token 数</summary>
    int? PromptTokens = null,

    /// <summary>生成了 Token 数</summary>
    int? CompletionTokens = null
);

/// <summary>
/// AI 消息
/// </summary>
public record AiMessage(
    /// <summary>角色：system / user / assistant</summary>
    string Role,

    /// <summary>消息内容</summary>
    string Content
);
```

### Provider 接口

```csharp
// YuG.Domain/Common/Interfaces/IAiProvider.cs

namespace YuG.Domain.Common.Interfaces;

/// <summary>
/// AI Provider 抽象，每个 AI 服务商实现此接口
/// </summary>
public interface IAiProvider
{
    /// <summary>Provider 名称（如 "OpenAI"、"Claude"）</summary>
    string Name { get; }

    /// <summary>发送对话请求</summary>
    Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>检查 Provider 是否可用</summary>
    bool IsAvailable();
}
```

### 响应解析器接口

```csharp
// YuG.Domain/Common/Interfaces/IResponseParser.cs

namespace YuG.Domain.Common.Interfaces;

/// <summary>
/// AI 响应内容解析器，每个业务场景实现一个解析器
/// </summary>
/// <typeparam name="T">解析结果类型</typeparam>
public interface IResponseParser<T>
{
    /// <summary>
    /// 从 AI 响应中提取结构化数据
    /// </summary>
    /// <param name="response">AI 完整回复内容</param>
    /// <returns>解析结果，无法解析返回 null</returns>
    T? Parse(string response);
}
```

### 枚举与配置

```csharp
// YuG.Domain/Common/Enums/AiProviderType.cs
public enum AiProviderType
{
    OpenAi,
    AnthropicClaude,
    Custom
}
```

---

## 基础设施层实现

### 目录结构

```
YuG.Infrastructure/Services/Ai/
├── AiService.cs                        ← IAiService 实现（统一入口）
├── Configuration/
│   └── AiOptions.cs                    ← 配置模型
├── Providers/
│   ├── IProviderClient.cs              ← Provider 客户端接口
│   ├── OpenAiProviderClient.cs         ← OpenAI 实现
│   ├── ClaudeProviderClient.cs         ← Anthropic Claude 实现
│   └── ProviderClientFactory.cs        ← 根据配置创建 Provider
├── Parsing/
│   ├── CodeBlockParser.cs              ← 通用代码块提取（```xxx ... ```）
│   └── JsonParser.cs                   ← JSON 结构提取
└── Extensions/
    └── AiServiceCollectionExtensions.cs ← DI 注册扩展
```

### AiService 实现

```csharp
namespace YuG.Infrastructure.Services.Ai;

/// <summary>
/// AI 服务实现
/// - 根据配置路由到对应的 Provider
/// - 处理超时、重试、降级
/// - 记录调用日志和指标
/// </summary>
public class AiService : IAiService
{
    private readonly IProviderClient _providerClient;
    private readonly ILogger<AiService> _logger;

    public AiService(
        IProviderClient providerClient,
        ILogger<AiService> logger)
    {
        _providerClient = providerClient;
        _logger = logger;
    }

    public async Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. 前置校验
        // 2. 调用 Provider（含重试策略）
        // 3. 记录调用日志
        // 4. 返回响应
    }
}
```

### Provider 客户端

```csharp
namespace YuG.Infrastructure.Services.Ai.Providers;

/// <summary>
/// Provider 客户端接口，封装与 AI API 的 HTTP 通信
/// </summary>
public interface IProviderClient
{
    Task<AiCompletionResponse> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<AiMessage> messages,
        AiRequestOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// OpenAI 实现（兼容 API）
/// </summary>
public class OpenAiProviderClient : IProviderClient { }

/// <summary>
/// Anthropic Claude 实现
/// </summary>
public class ClaudeProviderClient : IProviderClient { }
```

### 配置模型

```csharp
namespace YuG.Infrastructure.Services.Ai.Configuration;

/// <summary>
/// AI Provider 配置
/// </summary>
public class AiOptions
{
    /// <summary>默认 Provider：OpenAI / AnthropicClaude / Custom</summary>
    public string DefaultProvider { get; set; } = "OpenAI";

    /// <summary>各 Provider 的详细配置</summary>
    public Dictionary<string, ProviderConfig> Providers { get; set; } = [];

    /// <summary>
    /// 重试策略
    /// </summary>
    public RetryPolicyConfig RetryPolicy { get; set; } = new();
}

public class ProviderConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double DefaultTemperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4096;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}

public class RetryPolicyConfig
{
    public int MaxRetries { get; set; } = 3;
    public int BaseDelayMs { get; set; } = 1000;
}
```

### 响应解析器

```csharp
namespace YuG.Infrastructure.Services.Ai.Parsing;

/// <summary>
/// 通用代码块提取解析器
/// 从 AI 响应中提取 ```lang ... ``` 包裹的代码块
/// </summary>
public class CodeBlockParser : IResponseParser<CodeBlockResult>
{
    /// <param name="language">代码块语言标识，如 "mermaid"、"json"、"xml"</param>
    public CodeBlockParser(string language)
    {
        _pattern = $@"```{language}\s*\n([\s\S]*?)```";
    }

    public CodeBlockResult? Parse(string response)
    {
        // 正则匹配代码块
        // 返回代码内容和剩余文本（解释性内容）
    }
}

/// <summary>
/// 代码块解析结果
/// </summary>
public record CodeBlockResult(
    string? Code,        // 提取的代码内容
    string? Explanation  // 剩余的文本内容
);
```

### DI 注册

```csharp
// YuG.Infrastructure/Extensions/AiServiceCollectionExtensions.cs

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection("Ai"));

        // 注册 Provider 客户端
        services.AddSingleton<ProviderClientFactory>();
        services.AddSingleton<IProviderClient>(sp =>
            sp.GetRequiredService<ProviderClientFactory>().Create());

        // 注册统一 AI 服务
        services.AddSingleton<IAiService, AiService>();

        // 注册内置解析器
        services.AddSingleton<IResponseParser<CodeBlockResult>>(
            _ => new CodeBlockParser("mermaid"));

        return services;
    }
}
```

---

# 第二部分：FlowDesign 模块

## 领域模型

### 聚合：FlowProject（流程项目）

作为分组容器，用于组织多个流程图。

```csharp
namespace YuG.Domain.FlowDesign.Entities;

public class FlowProject : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public FlowProjectStatus Status { get; private set; }  // Active / Archived

    private FlowProject() { } // EF Core 构造

    public FlowProject(string name, string? description)
    {
        ValidateName(name);
        Name = name;
        Description = description;
        Status = FlowProjectStatus.Active;
    }

    public void Rename(string newName) { ... }
    public void ChangeDescription(string? description) { ... }
    public void Archive() { ... }
    public void Activate() { ... }
}
```

### 聚合：Flowchart（流程图）

包含 Mermaid DSL 源码，关联到 FlowProject。

```csharp
public class Flowchart : AggregateRoot
{
    public long ProjectId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string MermaidContent { get; private set; }
    public FlowchartStatus Status { get; private set; }  // Draft / Published

    private Flowchart() { }

    public Flowchart(long projectId, string title, string? description, string? mermaidContent) { ... }

    public void Rename(string newTitle) { ... }
    public void ChangeDescription(string? description) { ... }
    public void UpdateMermaidContent(string mermaidContent) { ... }
    public void Publish() { ... }
    public void Draft() { ... }
}
```

### 聚合：ChatConversation（AI 对话会话）

管理一个流程图的 AI 对话历史，使用 `OwnsMany` 持有 `ChatMessage` 集合。

```csharp
public class ChatConversation : AggregateRoot
{
    public long FlowchartId { get; private set; }
    public string? Title { get; private set; }
    private readonly List<ChatMessage> _messages = [];
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    private ChatConversation() { }

    public ChatConversation(long flowchartId) { ... }

    public void Rename(string? title) { ... }

    public void AddMessage(ChatMessageRole role, string content, string? mermaidCode = null)
    {
        _messages.Add(new ChatMessage(role, content, mermaidCode));
        UpdatedAt = DateTime.UtcNow;
    }
}

public class ChatMessage
{
    public long Id { get; private set; }
    public ChatMessageRole Role { get; private set; }  // User / Assistant
    public string Content { get; private set; }
    public string? MermaidCode { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ChatMessage() { }
    public ChatMessage(ChatMessageRole role, string content, string? mermaidCode = null) { ... }
}
```

### 枚举

| 枚举 | 值 |
|------|-----|
| FlowProjectStatus | Active, Archived |
| FlowchartStatus | Draft, Published |
| ChatMessageRole | User, Assistant |

### 领域事件

| 事件 | 触发时机 |
|------|----------|
| FlowProjectCreatedEvent | 项目创建 |
| FlowchartCreatedEvent | 流程图创建 |
| FlowchartMermaidUpdatedEvent | Mermaid 内容变更 |

### 仓储接口

| 接口 | 关键方法 |
|------|----------|
| IFlowProjectRepository | 标准 CRUD |
| IFlowchartRepository | GetByProjectIdAsync(), ExistsByProjectIdAsync() |
| IChatConversationRepository | GetByFlowchartIdAsync()（含 Messages） |

---

## 应用层

### FlowProject 用例

| 操作 | Command | 说明 |
|------|---------|------|
| 创建项目 | CreateFlowProjectCommand | — |
| 更新项目 | UpdateFlowProjectCommand | 改名/改描述 |
| 删除项目 | DeleteFlowProjectCommand | 级联删除 |
| 获取详情 | GetFlowProjectQuery | — |
| 获取列表 | GetFlowProjectListQuery | 分页+筛选 |
| 归档 | ArchiveFlowProjectCommand | — |
| 激活 | ActivateFlowProjectCommand | — |

### Flowchart 用例

| 操作 | Command | 说明 |
|------|---------|------|
| 创建流程图 | CreateFlowchartCommand | — |
| 更新信息 | UpdateFlowchartCommand | 改名/改描述 |
| 删除流程图 | DeleteFlowchartCommand | 级联删除对话 |
| 获取详情 | GetFlowchartQuery | 含 MermaidContent |
| 获取列表 | GetFlowchartListQuery | 按项目ID |
| 手动更新 Mermaid | UpdateFlowchartMermaidCommand | 手动微调 |

### Chat 用例

| 操作 | Command | 说明 |
|------|---------|------|
| 发送消息 | SendChatMessageCommand | **核心**：通过 AI 模块对话 |
| 获取对话历史 | GetConversationQuery | 全部消息 |

---

## 核心流程：AI 对话（SendMessageHandler）

这是 FlowDesign 如何调用 AI Core 的关键流程：

```
用户发送消息 → SendMessageHandler:

  1. 获取 Flowchart（含当前 MermaidContent）
     └─ 若流程图不存在 → 抛 NotFoundException

  2. 获取或创建 ChatConversation（按 FlowchartId）
     └─ 懒创建：首次发消息时生成

  3. 保存 User 消息到 Conversation

  4. 构建 System Prompt
     └─ 模板：
        你是一个专业的流程图设计助手。
        当前流程图 Mermaid 代码：
        ```mermaid
        {current_mermaid}
        ```
        根据用户的需求生成或修改 Mermaid.js 流程图代码。
        将 Mermaid 代码包裹在 ```mermaid 代码块中。

  5. 调用 IAiService.CompleteAsync()
     └─ 传入 SystemPrompt + 消息历史（转换为 AiMessage 格式）
     └─ AI Core 模块自动处理：Provider 路由 / 重试 / 日志

  6. 用 IResponseParser<CodeBlockResult> 解析 AI 响应
     └─ 提取 ```mermaid ... ``` 代码块中的 Mermaid DSL

  7. 保存 Assistant 消息（含解析出的 MermaidCode）

  8. 若解析出 Mermaid 代码 → flowchart.UpdateMermaidContent(newMermaid)

  9. SaveChangesAsync() 事务提交（消息 + 流程图）
```

### Handler 代码结构

```csharp
public class SendMessageHandler : IRequestHandler<SendChatMessageCommand, ChatMessageResult>
{
    private readonly IFlowchartRepository _flowchartRepo;
    private readonly IChatConversationRepository _conversationRepo;
    private readonly IAiService _aiService;
    private readonly IResponseParser<CodeBlockResult> _mermaidParser;

    public SendMessageHandler(
        IFlowchartRepository flowchartRepo,
        IChatConversationRepository conversationRepo,
        IAiService aiService,
        IResponseParser<CodeBlockResult> mermaidParser)
    {
        _flowchartRepo = flowchartRepo;
        _conversationRepo = conversationRepo;
        _aiService = aiService;
        _mermaidParser = mermaidParser;
    }

    public async Task<ChatMessageResult> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        // 1. 获取流程图
        var flowchart = await _flowchartRepo.GetByIdAsync(request.FlowchartId, cancellationToken)
            ?? throw new NotFoundException(nameof(Flowchart), request.FlowchartId);

        // 2. 获取或创建对话
        var conversation = await GetOrCreateConversation(request.FlowchartId, cancellationToken);

        // 3. 保存用户消息
        conversation.AddMessage(ChatMessageRole.User, request.Content);

        // 4. 构建 System Prompt
        var systemPrompt = BuildSystemPrompt(flowchart.MermaidContent);

        // 5. 转换消息格式 → AiMessage
        var aiMessages = conversation.Messages
            .Select(m => new AiMessage(
                m.Role == ChatMessageRole.User ? "user" : "assistant",
                m.Content))
            .ToList();

        // 6. 调用 AI Core
        var aiResponse = await _aiService.CompleteAsync(
            new AiCompletionRequest(aiMessages, systemPrompt),
            cancellationToken);

        // 7. 解析 Mermaid 代码
        var parsed = _mermaidParser.Parse(aiResponse.Content);
        var mermaidCode = parsed?.Code;

        // 8. 保存助理消息
        conversation.AddMessage(ChatMessageRole.Assistant, aiResponse.Content, mermaidCode);

        // 9. 更新流程图
        if (mermaidCode is not null)
            flowchart.UpdateMermaidContent(mermaidCode);

        // 10. 提交
        await _conversationRepo.SaveChangesAsync(cancellationToken);

        return new ChatMessageResult { ... };
    }
}
```

---

## 基础设施

### 数据库表结构

```
FlowProject            Flowchart              ChatConversation       ChatMessage
┌─────────────┐       ┌──────────────┐       ┌────────────────┐    ┌──────────────────┐
│ Id (PK)     │←──────│ ProjectId    │       │ Id (PK)        │    │ ConversationId   │
│ Name        │       │ Id (PK)      │←──────│ FlowchartId    │    │ Id (PK)          │
│ Description │       │ Title        │       │ Title          │    │ Role             │
│ Status      │       │ Description  │       │ CreatedAt      │    │ Content          │
│ CreatedAt   │       │ MermaidContent│      │ UpdatedAt      │    │ MermaidCode?     │
│ UpdatedAt   │       │ Status       │       └────────────────┘    │ CreatedAt        │
└─────────────┘       │ CreatedAt    │                             └──────────────────┘
                      │ UpdatedAt    │
                      └──────────────┘
```

### EF Core 配置

- `FlowProjectConfiguration`: `ToTable("FlowProject")`，Status 存字符串
- `FlowchartConfiguration`: `ToTable("Flowchart")`
- `ChatConversationConfiguration`: `ToTable("ChatConversation")`
  - `OwnsMany(c => c.Messages, m => m.ToTable("ChatMessage"))`

### 仓储实现

继承 `Repository<TAggregate>` 基类，按需扩展查询。

### DI 注册

在 `DependencyInjection.cs` 新增：

```csharp
// FlowDesign 仓储
services.AddScoped<IFlowProjectRepository, FlowProjectRepository>();
services.AddScoped<IFlowchartRepository, FlowchartRepository>();
services.AddScoped<IChatConversationRepository, ChatConversationRepository>();

// AI Core 模块
services.AddAiServices(configuration);
```

`appsettings.json` 配置：

```json
{
  "Ai": {
    "DefaultProvider": "OpenAI",
    "Providers": {
      "OpenAI": {
        "ApiKey": "",
        "BaseUrl": "https://api.openai.com/v1",
        "Model": "gpt-4o",
        "DefaultTemperature": 0.7,
        "MaxTokens": 4096,
        "Timeout": "00:01:00"
      },
      "AnthropicClaude": {
        "ApiKey": "",
        "BaseUrl": "https://api.anthropic.com",
        "Model": "claude-sonnet-4-20250514",
        "DefaultTemperature": 0.7,
        "MaxTokens": 4096,
        "Timeout": "00:01:00"
      }
    },
    "RetryPolicy": {
      "MaxRetries": 3,
      "BaseDelayMs": 1000
    }
  }
}
```

---

## API 层

### FlowProjectController — `api/flow-design/project`

| 方法 | 路由 | 权限编码 |
|------|------|----------|
| GET | `/api/flow-design/project` | `flow-project:get` |
| POST | `/api/flow-design/project` | `flow-project:create` |
| GET | `/api/flow-design/project/{id}` | `flow-project:getbyid` |
| PUT | `/api/flow-design/project/{id}` | `flow-project:update` |
| DELETE | `/api/flow-design/project/{id}` | `flow-project:delete` |
| POST | `/api/flow-design/project/{id}/archive` | `flow-project:archive` |
| POST | `/api/flow-design/project/{id}/activate` | `flow-project:activate` |

### FlowchartController — `api/flow-design/flowchart`

| 方法 | 路由 | 权限编码 |
|------|------|----------|
| GET | `/api/flow-design/flowchart/project/{projectId}` | `flowchart:get` |
| POST | `/api/flow-design/flowchart/project/{projectId}` | `flowchart:create` |
| GET | `/api/flow-design/flowchart/{id}` | `flowchart:getbyid` |
| PUT | `/api/flow-design/flowchart/{id}` | `flowchart:update` |
| DELETE | `/api/flow-design/flowchart/{id}` | `flowchart:delete` |
| PUT | `/api/flow-design/flowchart/{id}/mermaid` | `flowchart:updatemermaid` |

### ChatController — `api/flow-design/chat`

| 方法 | 路由 | 权限编码 |
|------|------|----------|
| GET | `/api/flow-design/chat/flowchart/{flowchartId}/conversation` | `flowchart:chat` |
| POST | `/api/flow-design/chat/flowchart/{flowchartId}/messages` | `flowchart:chat` |

---

## 文件清单

### FlowDesign 模块

```
Domain/
├── YuG.Domain/FlowDesign/
│   ├── Entities/FlowProject.cs
│   ├── Entities/Flowchart.cs
│   ├── Entities/ChatConversation.cs
│   ├── Enums/FlowProjectStatus.cs
│   ├── Enums/FlowchartStatus.cs
│   ├── Enums/ChatMessageRole.cs
│   ├── Events/FlowProjectCreatedEvent.cs
│   ├── Events/FlowchartCreatedEvent.cs
│   ├── Events/FlowchartMermaidUpdatedEvent.cs
│   ├── Repositories/IFlowProjectRepository.cs
│   ├── Repositories/IFlowchartRepository.cs
│   └── Repositories/IChatConversationRepository.cs

Application/
├── YuG.Application/FlowDesign/
│   ├── FlowProject/
│   │   ├── Create/     Command.cs + Handler.cs
│   │   ├── Update/     Command.cs + Handler.cs
│   │   ├── Delete/     Command.cs + Handler.cs
│   │   ├── Get/        Query.cs + Handler.cs
│   │   ├── GetList/    Query.cs + Handler.cs
│   │   ├── Archive/    Command.cs + Handler.cs
│   │   └── Activate/   Command.cs + Handler.cs
│   ├── Flowchart/
│   │   ├── Create/          Command.cs + Handler.cs
│   │   ├── Update/          Command.cs + Handler.cs
│   │   ├── Delete/          Command.cs + Handler.cs
│   │   ├── Get/             Query.cs + Handler.cs
│   │   ├── GetList/         Query.cs + Handler.cs
│   │   └── UpdateMermaid/   Command.cs + Handler.cs
│   ├── Chat/
│   │   ├── SendMessage/     Command.cs + Handler.cs
│   │   └── GetConversation/ Query.cs + Handler.cs

Infrastructure/
├── YuG.Infrastructure/Persistence/FlowDesign/
│   ├── Configurations/
│   │   ├── FlowProjectConfiguration.cs
│   │   ├── FlowchartConfiguration.cs
│   │   └── ChatConversationConfiguration.cs
│   └── Repositories/
│       ├── FlowProjectRepository.cs
│       ├── FlowchartRepository.cs
│       └── ChatConversationRepository.cs

Api/
├── YuG.Api/Controllers/FlowDesign/
│   ├── FlowProjectController.cs
│   ├── FlowchartController.cs
│   └── ChatController.cs
```

---

# 第三部分：设计决策说明

| 决策 | 选择 | 理由 |
|------|------|------|
| **ChatConversation 归属** | FlowDesign 模块 | 对话数据与流程图强相关，AICore 只做无状态 API 调用，不管理业务数据 |
| **Mermaid 存储方式** | 纯文本 VARCHAR | Mermaid DSL 是源码文本，直接存字符串，前端传 mermaid.js 渲染 |
| **事务一致性** | 单 SaveChangesAsync | AI 响应后，保存消息 + 更新流程图 Mermaid 在同一个事务中提交 |
