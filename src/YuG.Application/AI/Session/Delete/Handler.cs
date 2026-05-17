using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Interfaces;
using YuG.Domain.AI.Repositories;

namespace YuG.Application.AI.Session.Delete;

/// <summary>删除会话命令处理器。</summary>
public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, Unit>
{
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IChatService _chatService;
    private readonly IUserIdentity _userIdentity;

    /// <summary>初始化处理器。</summary>
    /// <param name="sessionRepository">会话仓储</param>
    /// <param name="chatService">AI 聊天服务</param>
    /// <param name="userIdentity">当前用户身份</param>
    public DeleteSessionCommandHandler(
        IAiChatSessionRepository sessionRepository,
        IChatService chatService,
        IUserIdentity userIdentity)
    {
        _sessionRepository = sessionRepository;
        _chatService = chatService;
        _userIdentity = userIdentity;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _userIdentity.UserId;

        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != userId)
            throw new NotFoundException(nameof(Domain.AI.Entities.AiChatSession), request.SessionId);

        _sessionRepository.Delete(session);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        // 尝试清除远程 AI Gateway 会话缓存，失败不影响本地删除
        try
        {
            await _chatService.ClearSessionAsync(request.SessionId);
        }
        catch
        {
            // 忽略远程清除失败
        }

        return Unit.Value;
    }
}
