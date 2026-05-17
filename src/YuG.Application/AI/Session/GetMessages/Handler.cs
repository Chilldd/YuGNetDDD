using MediatR;
using Microsoft.EntityFrameworkCore;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Interfaces;
using YuG.Common.Models;
using YuG.Domain.AI.Entities;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.Session.GetMessages;

/// <summary>获取会话消息历史查询处理器。</summary>
public class Handler : IRequestHandler<GetSessionMessagesQuery, PageResult<MessageItem>>
{
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IUserIdentity _userIdentity;

    /// <summary>初始化处理器。</summary>
    /// <param name="sessionRepository">会话仓储</param>
    /// <param name="userIdentity">当前用户身份</param>
    public Handler(
        IAiChatSessionRepository sessionRepository,
        IUserIdentity userIdentity)
    {
        _sessionRepository = sessionRepository;
        _userIdentity = userIdentity;
    }

    /// <inheritdoc />
    public async Task<PageResult<MessageItem>> Handle(GetSessionMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _userIdentity.UserId;

        var sessionExists = await _sessionRepository.GetQueryable()
            .AnyAsync(s => s.SessionId == request.SessionId && s.UserId == userId, cancellationToken);

        if (!sessionExists)
            throw new NotFoundException(nameof(AiChatSession), request.SessionId);

        var result = await _sessionRepository.GetMessagesPagedAsync(
            request.SessionId, request.Page, request.PageSize,
            m => new MessageItem
            {
                Role = m.Role,
                Content = m.Content,
                SequenceNumber = m.SequenceNumber,
                TokenCount = m.TokenCount,
                CreatedAt = m.CreatedAt,
            }, cancellationToken);

        return result with { Items = result.Items.OrderBy(i => i.SequenceNumber).ToList() };
    }
}
