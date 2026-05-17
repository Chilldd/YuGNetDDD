using YuG.Application.AI.Chat.Common;

namespace YuG.Application.Common.Interfaces;

/// <summary>AI 聊天服务接口，负责与 AI Gateway 通信。</summary>
public interface IChatService
{
    /// <summary>发送消息历史到 AI Gateway 获取回复。</summary>
    /// <param name="message">用户当前输入</param>
    /// <param name="history">历史消息列表</param>
    /// <param name="userId">用户标识</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>聊天回复结果</returns>
    Task<ChatReplyResult> ChatAsync(string message, List<ChatMessageDto> history, long userId, CancellationToken ct = default);

    /// <summary>流式聊天，返回 SSE 事件流。</summary>
    /// <param name="message">用户当前输入</param>
    /// <param name="history">历史消息列表</param>
    /// <param name="userId">用户标识</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>SSE 增量事件流</returns>
    IAsyncEnumerable<ChatStreamDeltaResult> StreamAsync(string message, List<ChatMessageDto> history, long userId, CancellationToken ct = default);
}
