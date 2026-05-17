namespace YuG.Application.AI.Chat.Common;

/// <summary>聊天消息 DTO，用于在 Application 层传递消息历史。</summary>
public record ChatMessageDto(string Role, string Content);
