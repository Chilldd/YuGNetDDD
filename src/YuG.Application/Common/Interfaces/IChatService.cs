using YuG.Application.AI.Chat.Common;

namespace YuG.Application.Common.Interfaces;

/// <summary>AI 聊天服务接口。</summary>
public interface IChatService
{
    /// <summary>发送聊天消息并获取回复。</summary>
    /// <param name="message">用户消息</param>
    /// <param name="sessionId">会话 ID，为空则服务端自动创建新会话</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>聊天回复结果</returns>
    Task<ChatReplyResult> ChatAsync(string message, string? sessionId, CancellationToken ct = default);

    /// <summary>流式聊天，返回 SSE 事件流。</summary>
    /// <param name="message">用户消息</param>
    /// <param name="sessionId">会话 ID，为空则服务端自动创建新会话</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>SSE 增量事件流</returns>
    IAsyncEnumerable<ChatStreamDeltaResult> StreamAsync(string message, string? sessionId, CancellationToken ct = default);
}
