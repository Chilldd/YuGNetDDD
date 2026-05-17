using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using YuG.Application.AI.Chat.Common;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Entities;
using YuG.Infrastructure.HttpClients.AIGateway;
using YuG.Infrastructure.HttpClients.AIGateway.Responses;
using YuG.Infrastructure.Persistence;
using InfrastructureChatRequest = YuG.Infrastructure.HttpClients.AIGateway.Requests.ChatRequest;
using InfrastructureChatMessageDto = YuG.Infrastructure.HttpClients.AIGateway.Requests.ChatMessageDto;

namespace YuG.Infrastructure.Services;

/// <summary>AI 聊天服务，管理会话历史并调用 <see cref="IChatClient"/> 与 AI.Gateway 通信。</summary>
public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;
    private readonly IUserIdentity _userIdentity;
    private readonly ApplicationDbContext _dbContext;
    private readonly string? _systemPrompt;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>初始化 <see cref="ChatService"/> 实例。</summary>
    /// <param name="chatClient">AI.Gateway Refit 客户端</param>
    /// <param name="userIdentity">当前用户身份</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="configuration">应用配置</param>
    public ChatService(
        IChatClient chatClient,
        IUserIdentity userIdentity,
        ApplicationDbContext dbContext,
        IConfiguration configuration)
    {
        _chatClient = chatClient;
        _userIdentity = userIdentity;
        _dbContext = dbContext;
        _systemPrompt = configuration["AI:SystemPrompt"];
    }

    /// <inheritdoc />
    public async Task<ChatReplyResult> ChatAsync(string message, string? sessionId, CancellationToken ct)
    {
        var userId = _userIdentity.UserId;
        var session = await LoadOrCreateSessionAsync(sessionId, userId, ct);

        // 构造消息列表：已有消息 + 当前用户输入
        var messages = session.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new InfrastructureChatMessageDto { Role = m.Role, Content = m.Content })
            .ToList();

        messages.Add(new InfrastructureChatMessageDto { Role = "user", Content = message });

        // 调 Gateway
        var request = new InfrastructureChatRequest
        {
            UserId = userId,
            Messages = messages,
        };

        var response = await _chatClient.ChatAsync(request, ct);

        // 保存消息到 DB
        session.AddMessage("user", message);
        session.AddMessage("assistant", response.Reply);
        await _dbContext.SaveChangesAsync(ct);

        return MapToResult(response, session.SessionId);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDeltaResult> StreamAsync(
        string message,
        string? sessionId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var userId = _userIdentity.UserId;
        var session = await LoadOrCreateSessionAsync(sessionId, userId, ct);

        // 构造消息列表
        var messages = session.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new InfrastructureChatMessageDto { Role = m.Role, Content = m.Content })
            .ToList();

        messages.Add(new InfrastructureChatMessageDto { Role = "user", Content = message });

        // 调 Gateway
        var request = new InfrastructureChatRequest
        {
            UserId = userId,
            Messages = messages,
        };

        var httpResponse = await _chatClient.StreamAsync(request, ct);
        httpResponse.EnsureSuccessStatusCode();

        // 解析 SSE 流，累积完整回复
        var fullReply = new System.Text.StringBuilder();

        using var stream = await httpResponse.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null)
                break;

            if (line.Length == 0)
                continue;

            if (!line.StartsWith("data: ", StringComparison.Ordinal))
                continue;

            var json = line[6..];
            var infraDelta = JsonSerializer.Deserialize<ChatStreamDelta>(json, JsonOptions);
            if (infraDelta is null)
                continue;

            if (infraDelta.Type == "delta" && infraDelta.Content is not null)
            {
                fullReply.Append(infraDelta.Content);
            }

            yield return new ChatStreamDeltaResult
            {
                Type = infraDelta.Type,
                Content = infraDelta.Content ?? string.Empty,
                Usage = infraDelta.Usage is not null
                    ? MapToUsageResult(infraDelta.Usage)
                    : null,
            };
        }

        // 流结束后保存消息到 DB
        if (fullReply.Length > 0)
        {
            session.AddMessage("user", message);
            session.AddMessage("assistant", fullReply.ToString());
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    /// <summary>加载已有会话或创建新会话。</summary>
    private async Task<ChatSession> LoadOrCreateSessionAsync(string? sessionId, long userId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var session = await _dbContext.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId, ct);

            if (session is not null)
            {
                session.Touch();
                return session;
            }
        }

        // 创建新会话
        var newSession = new ChatSession(userId, _systemPrompt);
        _dbContext.ChatSessions.Add(newSession);
        await _dbContext.SaveChangesAsync(ct);
        return newSession;
    }

    private static ChatReplyResult MapToResult(ChatReplyResponse source, string sessionId)
    {
        return new ChatReplyResult
        {
            Reply = source.Reply,
            SessionId = sessionId,
            Model = source.Model,
            Usage = source.Usage is not null
                ? MapToUsageResult(source.Usage)
                : null,
            TotalInTokens = 0,
            TotalOutTokens = 0,
        };
    }

    private static UsageDataResult MapToUsageResult(UsageData source)
    {
        return new UsageDataResult
        {
            InTokens = source.InTokens,
            OutTokens = source.OutTokens,
            TotalTokens = source.TotalTokens,
            PromptCacheHitTokens = source.PromptCacheHitTokens,
            PromptCacheMissTokens = source.PromptCacheMissTokens,
        };
    }
}
