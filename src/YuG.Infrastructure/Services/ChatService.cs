using System.Runtime.CompilerServices;
using System.Text.Json;
using YuG.Application.AI.Chat.DTOs;
using YuG.Application.Common.Interfaces;
using YuG.Infrastructure.HttpClients.AIGateway;
using YuG.Infrastructure.HttpClients.AIGateway.Responses;
using InfrastructureChatRequest = YuG.Infrastructure.HttpClients.AIGateway.Requests.ChatRequest;
using InfrastructureChatMessageDto = YuG.Infrastructure.HttpClients.AIGateway.Requests.ChatMessageDto;
using InfrastructureToolCallDto = YuG.Infrastructure.HttpClients.AIGateway.Requests.ToolCallDto;

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
            .Select(m => new InfrastructureChatMessageDto
            {
                Role = m.Role,
                Content = m.Content,
                ToolCalls = m.ToolCalls?.Select(tc => new InfrastructureToolCallDto
                {
                    Id = tc.Id,
                    FunctionName = tc.Name,
                    Arguments = tc.Arguments,
                }).ToList(),
                ToolCallId = m.ToolCallId,
            })
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
            ToolCalls = response.ToolCalls?.Select(tc => new ToolCallRecordResult
            {
                Id = tc.Id,
                Name = tc.Name,
                DisplayName = tc.DisplayName,
                Arguments = tc.Arguments,
                Result = tc.Result,
            }).ToList(),
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDeltaResult> StreamAsync(
        List<ChatMessageDto> messages,
        long userId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var infraMessages = messages
            .Select(m => new InfrastructureChatMessageDto
            {
                Role = m.Role,
                Content = m.Content,
                ToolCalls = m.ToolCalls?.Select(tc => new InfrastructureToolCallDto
                {
                    Id = tc.Id,
                    FunctionName = tc.Name,
                    Arguments = tc.Arguments,
                }).ToList(),
                ToolCallId = m.ToolCallId,
            })
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
                ToolCall = infraDelta.ToolCall is not null
                    ? new ToolCallDeltaResult
                    {
                        Id = infraDelta.ToolCall.Id,
                        Name = infraDelta.ToolCall.Name,
                        DisplayName = infraDelta.ToolCall.DisplayName,
                        Arguments = infraDelta.ToolCall.Arguments,
                    }
                    : null,
                ToolResult = infraDelta.ToolResult is not null
                    ? new ToolCallResultDeltaResult
                    {
                        Id = infraDelta.ToolResult.Id,
                        Name = infraDelta.ToolResult.Name,
                        DisplayName = infraDelta.ToolResult.DisplayName,
                        Content = infraDelta.ToolResult.Content,
                    }
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
