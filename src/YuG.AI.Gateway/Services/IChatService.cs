using Microsoft.SemanticKernel.ChatCompletion;
using YuG.AI.Gateway.Models.Responses;

namespace YuG.AI.Gateway.Services;

/// <summary>聊天服务接口。</summary>
public interface IChatService
{
    /// <summary>基于会话历史的聊天补全。自动将问题加入历史并保存 AI 回复。</summary>
    /// <param name="history">会话历史</param>
    /// <param name="question">用户提问</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>聊天回复响应</returns>
    Task<ChatReplyResponse> ChatWithHistoryAsync(ChatHistory history, string question, CancellationToken ct = default);

    /// <summary>基于会话历史的流式聊天补全。自动将问题加入历史并保存完整回复。</summary>
    /// <param name="history">会话历史</param>
    /// <param name="question">用户提问</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>增量内容枚举</returns>
    IAsyncEnumerable<ChatStreamDelta> ChatStreamWithHistoryAsync(ChatHistory history, string question, CancellationToken ct = default);
}
