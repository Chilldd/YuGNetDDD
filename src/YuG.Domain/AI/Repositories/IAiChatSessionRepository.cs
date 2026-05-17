using YuG.Common.Models;
using YuG.Domain.AI.Entities;
using YuG.Domain.AI.ValueObjects;
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

    /// <summary>分页获取会话消息（按序号倒序，Page 1 为最新消息）。</summary>
    /// <param name="sessionId">会话 ID</param>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>消息分页结果</returns>
    Task<PageResult<AiChatMessage>> GetMessagesPagedAsync(string sessionId, int page, int pageSize, CancellationToken cancellationToken = default);
}
