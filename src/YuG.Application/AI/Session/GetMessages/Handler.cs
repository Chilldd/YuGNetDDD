using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.Session.GetMessages;

/// <summary>获取会话消息历史查询处理器。</summary>
public class Handler : IRequestHandler<GetSessionMessagesQuery, GetSessionMessagesResult>
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
    public async Task<GetSessionMessagesResult> Handle(GetSessionMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _userIdentity.UserId;

        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != userId)
            throw new NotFoundException(nameof(Domain.AI.Entities.AiChatSession), request.SessionId);

        var orderedMessages = session.Messages.OrderBy(m => m.SequenceNumber).ToList();
        var totalCount = orderedMessages.Count;

        // 从最新消息往回分页：Page 1 = 最后 PageSize 条
        var skip = Math.Max(0, totalCount - request.Page * request.PageSize);
        var take = Math.Min(request.PageSize, totalCount - skip);

        var items = orderedMessages
            .Skip(skip)
            .Take(take)
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new MessageItem
            {
                Role = m.Role,
                Content = m.Content,
                SequenceNumber = m.SequenceNumber,
                TokenCount = m.TokenCount,
                CreatedAt = m.CreatedAt,
            })
            .ToList();

        return new GetSessionMessagesResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
