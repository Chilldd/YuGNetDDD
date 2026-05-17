using System.Runtime.CompilerServices;
using System.Text.Json;
using YuG.Application.AI.Chat.Common;
using YuG.Application.Common.Interfaces;
using YuG.Infrastructure.HttpClients.AIGateway;
using YuG.Infrastructure.HttpClients.AIGateway.Responses;
using InfrastructureChatRequest = YuG.Infrastructure.HttpClients.AIGateway.Requests.ChatRequest;
using InfrastructureChatMessageDto = YuG.Infrastructure.HttpClients.AIGateway.Requests.ChatMessageDto;

namespace YuG.Infrastructure.Services;

/// <summary>AI 聊天服务，包装 <see cref="IChatClient"/> 与 AI.Gateway 通信。</summary>
public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>初始化 <see cref="ChatService"/> 实例。</summary>
    /// <param name="chatClient">AI.Gateway Refit 客户端</param>
    public ChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    /// <inheritdoc />
    public async Task<ChatReplyResult> ChatAsync(List<ChatMessageDto> messages, long userId, CancellationToken ct)
    {
        var infraMessages = messages
            .Select(m => new InfrastructureChatMessageDto { Role = m.Role, Content = m.Content })
            .ToList();

        var request = new InfrastructureChatRequest
        {
            UserId = userId,
            Messages = infraMessages,
        };

        var response = await _chatClient.ChatAsync(request, ct);

        return new ChatReplyResult
        {
            Reply = response.Reply,
            Model = response.Model,
            Usage = response.Usage is not null ? MapToUsageResult(response.Usage) : null,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDeltaResult> StreamAsync(
        List<ChatMessageDto> messages,
        long userId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var infraMessages = messages
            .Select(m => new InfrastructureChatMessageDto { Role = m.Role, Content = m.Content })
            .ToList();

        var request = new InfrastructureChatRequest
        {
            UserId = userId,
            Messages = infraMessages,
        };

        var httpResponse = await _chatClient.StreamAsync(request, ct);
        httpResponse.EnsureSuccessStatusCode();

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

            yield return new ChatStreamDeltaResult
            {
                Type = infraDelta.Type,
                Content = infraDelta.Content ?? string.Empty,
                Usage = infraDelta.Usage is not null
                    ? MapToUsageResult(infraDelta.Usage)
                    : null,
            };
        }
    }

    /// <inheritdoc />
    public Task ClearSessionAsync(string sessionId)
    {
        return _chatClient.ClearSessionAsync(sessionId);
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
