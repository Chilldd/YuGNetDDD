using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YuG.Application.AI.Chat.DTOs;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Entities;
using YuG.Domain.AI.Events;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.EventHandlers;

/// <summary>首次对话完成后，调用 AI 生成会话标题。</summary>
public class SessionTitleGenerationEventHandler : INotificationHandler<SessionTitleGenerationRequested>
{
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IChatService _chatService;
    private readonly string _systemPrompt;
    private readonly ILogger<SessionTitleGenerationEventHandler> _logger;

    /// <summary>初始化处理器。</summary>
    /// <param name="sessionRepository">会话仓储</param>
    /// <param name="chatService">AI 聊天服务</param>
    /// <param name="configuration">应用配置</param>
    /// <param name="logger">日志记录器</param>
    public SessionTitleGenerationEventHandler(
        IAiChatSessionRepository sessionRepository,
        IChatService chatService,
        IConfiguration configuration,
        ILogger<SessionTitleGenerationEventHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _chatService = chatService;
        _systemPrompt = configuration["AI:TitleGenerationPrompt"]!;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(SessionTitleGenerationRequested notification, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionRepository.GetBySessionIdAsync(notification.SessionId, cancellationToken);
            if (session is null || session.Title != AiChatSession.DefaultTitle)
                return;

            var messages = session.Messages.OrderBy(m => m.SequenceNumber).ToList();
            var firstUserMsg = messages.FirstOrDefault(m => m.Role == "user");
            var firstAssistantMsg = messages.FirstOrDefault(m => m.Role == "assistant");
            if (firstUserMsg is null || firstAssistantMsg is null)
                return;

            var titleMessages = new List<ChatMessageDto>
            {
                new("system", _systemPrompt),
                new("user", firstUserMsg.Content),
                new("assistant", firstAssistantMsg.Content),
            };

            var reply = await _chatService.ChatAsync(titleMessages, notification.UserId, cancellationToken);
            var title = reply.Reply.Trim().Trim('"', '\'');

            if (!string.IsNullOrEmpty(title))
            {
                session.Rename(title);
                await _sessionRepository.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("会话 {SessionId} 标题已自动生成为: {Title}", notification.SessionId, title);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "会话 {SessionId} 标题自动生成失败，将在下次消息时重试", notification.SessionId);
        }
    }
}
