using YuG.Domain.AI.Entities;
using YuG.Domain.Common;

namespace YuG.Domain.AI.Repositories;

/// <summary>聊天会话仓储接口。</summary>
public interface IAiChatSessionRepository : IRepository<AiChatSession>
{
    /// <summary>根据外部会话 ID 获取会话（含消息）。</summary>
    /// <param name="sessionId">会话 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话，不存在返回 null</returns>
    Task<AiChatSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
}
