using Dapper;
using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Interfaces;
using YuG.Common.Models;

namespace YuG.Application.AI.Session.Queries.GetMessages;

/// <summary>获取会话消息历史查询处理器。</summary>
public class Handler : IRequestHandler<GetSessionMessagesQuery, PageResult<MessageItem>>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IUserIdentity _userIdentity;

    /// <summary>初始化处理器。</summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    /// <param name="userIdentity">当前用户身份</param>
    public Handler(
        ISqlConnectionFactory connectionFactory,
        IUserIdentity userIdentity)
    {
        _connectionFactory = connectionFactory;
        _userIdentity = userIdentity;
    }

    /// <inheritdoc />
    public async Task<PageResult<MessageItem>> Handle(GetSessionMessagesQuery request, CancellationToken cancellationToken)
    {
        using var conn = _connectionFactory.CreateConnection();

        // 检查会话是否存在且属于当前用户
        var sessionPkId = await conn.QueryFirstOrDefaultAsync<long?>(
            "SELECT Id FROM AiChatSession WHERE SessionId = @SessionId AND UserId = @UserId",
            new { request.SessionId, UserId = _userIdentity.UserId });

        if (sessionPkId is null)
            throw new NotFoundException(nameof(Domain.AI.Entities.AiChatSession), request.SessionId);

        // 查询总数
        var totalCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM AiChatMessage WHERE AiChatSessionId = @SessionPkId",
            new { SessionPkId = sessionPkId.Value });

        // 分页查询消息（倒序，Page 1 为最新）
        var offset = (request.Page - 1) * request.PageSize;
        var items = await conn.QueryAsync<MessageItem>(
            """
            SELECT Role, Content, SequenceNumber, TokenCount, CreatedAt
            FROM AiChatMessage
            WHERE AiChatSessionId = @SessionPkId
            ORDER BY Id DESC
            LIMIT @PageSize OFFSET @Offset
            """,
            new { SessionPkId = sessionPkId.Value, request.PageSize, Offset = offset });

        return new PageResult<MessageItem>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
