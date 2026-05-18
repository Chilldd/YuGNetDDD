using System.Runtime.CompilerServices;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using YuG.Application.AI.Chat.DTOs;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Entities;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.Chat.Commands.Stream;

/// <summary>流式聊天命令处理器。编排流程：加载会话 → 调 AI 流式服务 → 流结束后持久化消息。</summary>
public class StreamChatCommandHandler : IRequestHandler<StreamChatCommand, IAsyncEnumerable<ChatStreamDeltaResult>>
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
    public StreamChatCommandHandler(
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
    public async Task<IAsyncEnumerable<ChatStreamDeltaResult>> Handle(StreamChatCommand request, CancellationToken cancellationToken)
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

        // 3. 调 AI Gateway（流式）
        var rawStream = _chatService.StreamAsync(messages, userId, cancellationToken);

        // 4. 包装流：流结束后持久化消息
        return WrapStreamWithPersistence(rawStream, session, request.Message, cancellationToken);
    }

    private async IAsyncEnumerable<ChatStreamDeltaResult> WrapStreamWithPersistence(
        IAsyncEnumerable<ChatStreamDeltaResult> rawStream,
        AiChatSession session,
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var fullReply = new StringBuilder();

        await foreach (var delta in rawStream.WithCancellation(ct))
        {
            if (delta.Type == "delta" && delta.Content is not null)
                fullReply.Append(delta.Content);
            yield return delta;
        }

        // 流结束后保存消息
        if (fullReply.Length > 0)
        {
            session.AddMessage("user", userMessage);
            session.AddMessage("assistant", fullReply.ToString());
            await _sessionRepository.SaveChangesAsync(ct);
        }
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
