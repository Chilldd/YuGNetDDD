using MediatR;
using Microsoft.Extensions.Configuration;
using YuG.Application.AI.Chat.DTOs;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Entities;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.Chat.Commands.Send;

/// <summary>聊天消息命令处理器。负责编排业务流程：加载/创建会话 → 调 AI 服务 → 持久化消息。</summary>
public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatReplyResult>
{
    private readonly IChatService _chatService;
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IUserIdentity _userIdentity;
    private readonly string? _systemPrompt;

    /// <summary>初始化处理器。</summary>
    /// <param name="chatService">AI 聊天服务</param>
    /// <param name="sessionRepository">会话仓储</param>
    /// <param name="userIdentity">当前用户身份</param>
    /// <param name="configuration">应用配置</param>
    public ChatCommandHandler(
        IChatService chatService,
        IAiChatSessionRepository sessionRepository,
        IUserIdentity userIdentity,
        IConfiguration configuration)
    {
        _chatService = chatService;
        _sessionRepository = sessionRepository;
        _userIdentity = userIdentity;
        _systemPrompt = configuration["AI:SystemPrompt"];
    }

    /// <inheritdoc />
    public async Task<ChatReplyResult> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        var userId = _userIdentity.UserId;

        // 1. 加载或创建会话
        var session = await LoadOrCreateSessionAsync(request.SessionId, userId, cancellationToken);

        // 2. 构造完整消息列表（历史 + 当前用户输入）
        var messages = session.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new ChatMessageDto(m.Role, m.Content))
            .ToList();

        messages.Add(new ChatMessageDto("user", request.Message));

        // 3. 调 AI Gateway
        var result = await _chatService.ChatAsync(messages, userId, cancellationToken);

        // 4. 持久化消息
        session.AddMessage("user", request.Message);
        session.AddMessage("assistant", result.Reply);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        return result with { SessionId = session.SessionId };
    }

    private async Task<AiChatSession> LoadOrCreateSessionAsync(string? sessionId, long userId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var existing = await _sessionRepository.GetBySessionIdAsync(sessionId, ct);
            if (existing is not null)
            {
                existing.Touch();
                return existing;
            }
        }

        var session = new AiChatSession(userId, _systemPrompt);
        await _sessionRepository.AddAsync(session, ct);
        return session;
    }
}
