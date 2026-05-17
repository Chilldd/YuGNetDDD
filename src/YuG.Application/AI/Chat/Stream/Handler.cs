using MediatR;
using YuG.Application.AI.Chat.Common;
using YuG.Application.Common.Interfaces;

namespace YuG.Application.AI.Chat.Stream;

/// <summary>流式聊天命令处理器。</summary>
public class StreamChatCommandHandler : IRequestHandler<StreamChatCommand, IAsyncEnumerable<ChatStreamDeltaResult>>
{
    private readonly IChatService _chatService;

    /// <summary>初始化处理器。</summary>
    /// <param name="chatService">AI 聊天服务</param>
    public StreamChatCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <inheritdoc />
    public Task<IAsyncEnumerable<ChatStreamDeltaResult>> Handle(StreamChatCommand request, CancellationToken cancellationToken)
    {
        var stream = _chatService.StreamAsync(request.Message, request.SessionId, cancellationToken);
        return Task.FromResult(stream);
    }
}
