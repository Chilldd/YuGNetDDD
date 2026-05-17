using Microsoft.EntityFrameworkCore;
using YuG.Common.Extensions;
using YuG.Common.Models;
using YuG.Domain.AI.Entities;
using YuG.Domain.AI.Repositories;
using YuG.Domain.AI.ValueObjects;
using YuG.Domain.Common;

namespace YuG.Infrastructure.Persistence.AI.Repositories;

/// <summary>聊天会话仓储实现。</summary>
public class AiChatSessionRepository : Repository<AiChatSession>, IAiChatSessionRepository
{
    /// <summary>初始化 <see cref="AiChatSessionRepository"/> 实例。</summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="domainEventPublisher">领域事件发布器</param>
    public AiChatSessionRepository(ApplicationDbContext context, IDomainEventPublisher domainEventPublisher)
        : base(context, domainEventPublisher)
    {
    }

    /// <inheritdoc />
    public async Task<AiChatSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PageResult<AiChatMessage>> GetMessagesPagedAsync(string sessionId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(s => s.SessionId == sessionId)
            .SelectMany(s => s.Messages)
            .Where(m => m.Role != "system");

        return await query
            .OrderByDescending(m => m.SequenceNumber)
            .ToPageResultAsync(page, pageSize, cancellationToken);
    }
}
