using Microsoft.SemanticKernel.ChatCompletion;

namespace YuG.AI.Gateway.Services;

/// <summary>会话管理接口，负责创建和清理对话会话。</summary>
public interface ISessionService
{
    /// <summary>获取或创建会话。指定 <paramref name="sessionId"/> 时获取已有会话，否则创建新会话。</summary>
    /// <param name="sessionId">会话 ID，为空则自动生成</param>
    /// <returns>会话历史与 sessionId</returns>
    (ChatHistory History, string SessionId) GetOrCreateSession(string? sessionId);

    /// <summary>清空指定会话的历史记录。</summary>
    /// <param name="sessionId">会话 ID</param>
    void ClearSession(string sessionId);
}
