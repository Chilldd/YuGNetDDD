using YuG.AI.Gateway.Models.Requests;
using YuG.AI.Gateway.Models.Responses;

namespace YuG.AI.Gateway.Services;

/// <summary>聊天服务接口。</summary>
public interface IChatService
{
    /// <summary>基于完整消息历史的聊天补全。</summary>
    /// <param name="messages">消息历史列表，包含 system/user/assistant</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>聊天回复响应</returns>
    Task<ChatReplyResponse> ChatAsync(List<ChatMessageDto> messages, CancellationToken ct = default);

    /// <summary>基于完整消息历史的流式聊天补全。</summary>
    /// <param name="messages">消息历史列表，包含 system/user/assistant</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>增量内容枚举</returns>
    IAsyncEnumerable<ChatStreamDelta> ChatStreamAsync(List<ChatMessageDto> messages, CancellationToken ct = default);
}
