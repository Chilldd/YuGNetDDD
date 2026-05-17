using Refit;
using YuG.Infrastructure.HttpClients.AIGateway.Requests;
using YuG.Infrastructure.HttpClients.AIGateway.Responses;

namespace YuG.Infrastructure.HttpClients.AIGateway;

/// <summary>AI Gateway 聊天接口的 Refit 客户端。</summary>
public interface IChatClient
{
    /// <summary>基于会话的聊天补全，服务端自动管理上下文。</summary>
    /// <param name="request">聊天请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>回复与 sessionId</returns>
    [Post("/api/v1/chat")]
    Task<ChatReplyResponse> ChatAsync([Body] ChatRequest request, CancellationToken ct = default);

    /// <summary>基于会话的流式聊天补全（SSE 协议）。</summary>
    /// <param name="request">聊天请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>包含 SSE 事件流的响应</returns>
    [Post("/api/v1/chat/stream")]
    Task<HttpResponseMessage> StreamAsync([Body] ChatRequest request, CancellationToken ct = default);

    /// <summary>清空指定会话的历史记录。</summary>
    /// <param name="sessionId">会话 ID</param>
    [Delete("/api/v1/chat/session/{sessionId}")]
    Task ClearSessionAsync(string sessionId);
}
