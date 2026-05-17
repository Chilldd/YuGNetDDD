using System.Collections.Concurrent;
using Microsoft.SemanticKernel.ChatCompletion;

namespace YuG.AI.Gateway.Services;

/// <summary>基于内存的会话管理实现。</summary>
public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, ChatHistory> _sessions = new();

    /// <inheritdoc />
    public (ChatHistory History, string SessionId) GetOrCreateSession(string? sessionId)
    {
        if (sessionId is not null && _sessions.TryGetValue(sessionId, out var existing))
            return (existing, sessionId);

        var id = sessionId ?? Guid.NewGuid().ToString("N");
        var history = _sessions.GetOrAdd(id, _ => new ChatHistory());
        return (history, id);
    }

    /// <inheritdoc />
    public void ClearSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }
}
