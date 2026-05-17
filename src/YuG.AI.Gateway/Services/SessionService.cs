using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using YuG.AI.Gateway.Configuration;

namespace YuG.AI.Gateway.Services;

/// <summary>基于内存的会话管理实现。</summary>
public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, SessionData> _sessions = new();
    private readonly string? _systemPrompt;

    /// <summary>初始化 <see cref="SessionService"/> 实例。</summary>
    /// <param name="options">AI 配置选项，用于读取系统提示词</param>
    public SessionService(IOptions<AiOptions> options)
    {
        _systemPrompt = options.Value.SystemPrompt;
    }

    /// <inheritdoc />
    public (SessionData Session, string SessionId) GetOrCreateSession(string? sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId) && _sessions.TryGetValue(sessionId, out var existing))
        {
            existing.LastActivityAt = DateTime.UtcNow;
            return (existing, sessionId);
        }

        var id = Guid.NewGuid().ToString("N");
        var data = _sessions.GetOrAdd(id, _ =>
        {
            var sd = new SessionData();
            if (!string.IsNullOrWhiteSpace(_systemPrompt))
                sd.History.AddSystemMessage(_systemPrompt);
            return sd;
        });
        return (data, id);
    }

    /// <inheritdoc />
    public void ClearSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }
}
