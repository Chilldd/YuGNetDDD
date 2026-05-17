using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Models.Responses;

namespace YuG.AI.Gateway.Services;

/// <summary>聊天服务实现，使用 Semantic Kernel 的 <see cref="IChatCompletionService"/> 和 <see cref="ChatHistory"/> 管理对话。</summary>
public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly AiOptions _options;

    /// <summary>初始化 <see cref="ChatService"/> 实例。</summary>
    /// <param name="kernel">Semantic Kernel 实例</param>
    /// <param name="options">AI 配置选项</param>
    public ChatService(Kernel kernel, IOptions<AiOptions> options)
    {
        _kernel = kernel;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<ChatReplyResponse> ChatWithHistoryAsync(
        ChatHistory history, string question, CancellationToken ct = default)
    {
        history.AddUserMessage(question);

        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        var results = await chatCompletion.GetChatMessageContentsAsync(history, null, null, ct);

        var reply = results is [.., var last] ? last.Content ?? string.Empty : string.Empty;

        if (reply.Length > 0)
            history.AddAssistantMessage(reply);

        return new ChatReplyResponse
        {
            Reply = reply,
            Model = results.LastOrDefault()?.ModelId ?? _options.DeepSeek.ModelId,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamDelta> ChatStreamWithHistoryAsync(
        ChatHistory history, string question, [EnumeratorCancellation] CancellationToken ct = default)
    {
        history.AddUserMessage(question);

        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        var fullReply = new StringBuilder();

        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(history, null, null, ct))
        {
            if (content.Content is { Length: > 0 } text)
            {
                fullReply.Append(text);
                yield return new ChatStreamDelta(text);
            }
        }

        if (fullReply.Length > 0)
            history.AddAssistantMessage(fullReply.ToString());
    }
}
