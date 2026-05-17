using MediatR;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.Session.List;

/// <summary>会话列表查询处理器。</summary>
public class SessionListQueryHandler : IRequestHandler<SessionListQuery, IReadOnlyList<SessionListItemResult>>
{
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IUserIdentity _userIdentity;

    /// <summary>初始化处理器。</summary>
    /// <param name="sessionRepository">会话仓储</param>
    /// <param name="userIdentity">当前用户身份</param>
    public SessionListQueryHandler(
        IAiChatSessionRepository sessionRepository,
        IUserIdentity userIdentity)
    {
        _sessionRepository = sessionRepository;
        _userIdentity = userIdentity;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SessionListItemResult>> Handle(SessionListQuery request, CancellationToken cancellationToken)
    {
        var userId = _userIdentity.UserId;

        var sessions = await _sessionRepository.FindAsync(s => s.UserId == userId, cancellationToken);

        return sessions
            .OrderByDescending(s => s.LastActiveAt)
            .Select(s => new SessionListItemResult
            {
                SessionId = s.SessionId,
                Title = s.Title,
                LastActiveAt = s.LastActiveAt,
            })
            .ToList();
    }
}
