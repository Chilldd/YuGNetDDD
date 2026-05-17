using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Models.Responses;

namespace YuG.AI.Gateway.Services;

/// <summary>聊天服务实现，封装 <see cref="IChatClient"/> 的调用，使用 <see cref="ChatHistory"/> 管理对话上下文。</summary>
public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;
    private readonly AiOptions _options;

    /// <summary>初始化 <see cref="ChatService"/> 实例。</summary>
    /// <param name="chatClient">AI 聊天客户端</param>
    /// <param name="options">AI 配置选项</param>
    public ChatService(IChatClient chatClient, IOptions<AiOptions> options)
    {
        _chatClient = chatClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<ChatReplyResponse> ChatWithHistoryAsync(
        ChatHistory history, string question, CancellationToken ct = default)
    {
        history.AddUserMessage(question);

        var messages = history.Select(m => new Microsoft.Extensions.AI.ChatMessage(
            new ChatRole(m.Role.Label),
            m.Content ?? string.Empty))
            .ToList();

        var response = await _chatClient.GetResponseAsync(messages, null, ct);

        var reply = response.Messages
            .LastOrDefault(m => m.Role.Value == "assistant")
            ?.Text ?? string.Empty;

        if (reply.Length > 0)
            history.AddAssistantMessage(reply);

        return new ChatReplyResponse
        {
            Reply = reply,
            Model = response.ModelId ?? _options.DeepSeek.ModelId,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDelta> ChatStreamWithHistoryAsync(
        ChatHistory history, string question, [EnumeratorCancellation] CancellationToken ct = default)
    {
        history.AddUserMessage(question);

        var messages = history.Select(m => new Microsoft.Extensions.AI.ChatMessage(
            new ChatRole(m.Role.Label),
            m.Content ?? string.Empty))
            .ToList();

        var fullReply = new System.Text.StringBuilder();

        await foreach (var delta in _chatClient.GetStreamingResponseAsync(messages, null, ct))
        {
            if (delta.Contents is { Count: > 0 } contents)
            {
                foreach (var content in contents)
                {
                    if (content is TextContent text)
                    {
                        fullReply.Append(text.Text);
                        yield return new ChatStreamDelta(text.Text);
                    }
                }
            }
        }

        if (fullReply.Length > 0)
            history.AddAssistantMessage(fullReply.ToString());
    }
}
