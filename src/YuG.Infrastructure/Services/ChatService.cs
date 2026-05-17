using System.Runtime.CompilerServices;
using System.Text.Json;
using YuG.Application.AI.Chat.Common;
using YuG.Application.Common.Interfaces;
using YuG.Infrastructure.HttpClients.AIGateway;
using InfrastructureChatRequest = YuG.Infrastructure.HttpClients.AIGateway.Requests.ChatRequest;
using InfrastructureChatReplyResponse = YuG.Infrastructure.HttpClients.AIGateway.Responses.ChatReplyResponse;
using InfrastructureChatStreamDelta = YuG.Infrastructure.HttpClients.AIGateway.Responses.ChatStreamDelta;
using InfrastructureUsageData = YuG.Infrastructure.HttpClients.AIGateway.Responses.UsageData;

namespace YuG.Infrastructure.Services;

/// <summary>AI 聊天服务，包装 <see cref="IChatClient"/> Refit 客户端。</summary>
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
    public async Task<ChatReplyResult> ChatAsync(string message, string? sessionId, CancellationToken ct)
    {
        var request = new InfrastructureChatRequest
        {
            Message = message,
            SessionId = sessionId,
        };

        var response = await _chatClient.ChatAsync(request, ct);

        return MapToResult(response);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDeltaResult> StreamAsync(
        string message,
        string? sessionId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var request = new InfrastructureChatRequest
        {
            Message = message,
            SessionId = sessionId,
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

            var json = line[6..]; // strip "data: " prefix
            var infraDelta = JsonSerializer.Deserialize<InfrastructureChatStreamDelta>(json, JsonOptions);
            if (infraDelta is null)
                continue;

            yield return new ChatStreamDeltaResult
            {
                Type = infraDelta.Type,
                Content = infraDelta.Content,
                Usage = infraDelta.Usage is not null
                    ? MapToUsageResult(infraDelta.Usage)
                    : null,
            };
        }
    }

    private static ChatReplyResult MapToResult(InfrastructureChatReplyResponse source)
    {
        return new ChatReplyResult
        {
            Reply = source.Reply,
            SessionId = source.SessionId,
            Model = source.Model,
            Usage = source.Usage is not null
                ? MapToUsageResult(source.Usage)
                : null,
            TotalInTokens = source.TotalInTokens,
            TotalOutTokens = source.TotalOutTokens,
        };
    }

    private static UsageDataResult MapToUsageResult(InfrastructureUsageData source)
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
