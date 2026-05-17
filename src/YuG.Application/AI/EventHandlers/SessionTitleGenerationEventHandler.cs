using MediatR;
using YuG.Application.AI.Chat.Common;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Events;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.EventHandlers;

/// <summary>首次对话完成后，调用 AI 生成会话标题。</summary>
public class SessionTitleGenerationEventHandler : INotificationHandler<SessionTitleGenerationRequested>
{
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IChatService _chatService;

    /// <summary>初始化处理器。</summary>
    /// <param name="sessionRepository">会话仓储</param>
    /// <param name="chatService">AI 聊天服务</param>
    public SessionTitleGenerationEventHandler(
        IAiChatSessionRepository sessionRepository,
        IChatService chatService)
    {
        _sessionRepository = sessionRepository;
        _chatService = chatService;
    }

    /// <inheritdoc />
    public async Task Handle(SessionTitleGenerationRequested notification, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionRepository.GetBySessionIdAsync(notification.SessionId, cancellationToken);
            if (session is null || session.Title != "新对话")
                return;

            var messages = session.Messages.OrderBy(m => m.SequenceNumber).ToList();
            var firstUserMsg = messages.FirstOrDefault(m => m.Role == "user");
            var firstAssistantMsg = messages.FirstOrDefault(m => m.Role == "assistant");
            if (firstUserMsg is null || firstAssistantMsg is null)
                return;

            var titleMessages = new List<ChatMessageDto>
            {
                new("system", "根据以下对话内容生成一个简短的会话标题，不超过6个字。只返回标题文本，不要有多余内容。"),
                new("user", firstUserMsg.Content),
                new("assistant", firstAssistantMsg.Content),
            };

            var reply = await _chatService.ChatAsync(titleMessages, notification.UserId, cancellationToken);
            var title = reply.Reply.Trim().Trim('"', '\'');

            if (!string.IsNullOrEmpty(title))
            {
                session.Rename(title);
                await _sessionRepository.SaveChangesAsync(cancellationToken);
            }
        }
        catch
        {
            // 标题生成失败不影响正常聊天流程，下次消息会重试
        }
    }
}
