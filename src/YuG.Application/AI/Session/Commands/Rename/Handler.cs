using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.Session.Commands.Rename;

/// <summary>重命名会话命令处理器。</summary>
public class RenameSessionCommandHandler : IRequestHandler<RenameSessionCommand, Unit>
{
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IUserIdentity _userIdentity;

    /// <summary>初始化处理器。</summary>
    /// <param name="sessionRepository">会话仓储</param>
    /// <param name="userIdentity">当前用户身份</param>
    public RenameSessionCommandHandler(
        IAiChatSessionRepository sessionRepository,
        IUserIdentity userIdentity)
    {
        _sessionRepository = sessionRepository;
        _userIdentity = userIdentity;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(RenameSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _userIdentity.UserId;

        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != userId)
            throw new NotFoundException(nameof(Domain.AI.Entities.AiChatSession), request.SessionId);

        session.Rename(request.Title);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
