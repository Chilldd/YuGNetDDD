using Dapper;
using MediatR;
using YuG.Application.Common.Interfaces;

namespace YuG.Application.AI.Session.Queries.GetList;

/// <summary>获取会话列表查询处理器。</summary>
public class Handler : IRequestHandler<GetSessionListQuery, IReadOnlyList<SessionListItem>>
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
    public async Task<IReadOnlyList<SessionListItem>> Handle(GetSessionListQuery request, CancellationToken cancellationToken)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var sessions = await conn.QueryAsync<SessionListItem>(
            """
            SELECT SessionId, Title, LastActiveAt
            FROM AiChatSession
            WHERE UserId = @UserId
            ORDER BY LastActiveAt DESC
            """,
            new { UserId = _userIdentity.UserId });

        return sessions.ToList();
    }
}
