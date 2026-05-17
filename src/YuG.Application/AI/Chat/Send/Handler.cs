using MediatR;
using YuG.Application.AI.Chat.Common;
using YuG.Application.Common.Interfaces;

namespace YuG.Application.AI.Chat.Send;

/// <summary>聊天消息命令处理器。</summary>
public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatReplyResult>
{
    private readonly IChatService _chatService;

    /// <summary>初始化处理器。</summary>
    /// <param name="chatService">AI 聊天服务</param>
    public ChatCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <inheritdoc />
    public async Task<ChatReplyResult> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        return await _chatService.ChatAsync(request.Message, request.SessionId, cancellationToken);
    }
}
