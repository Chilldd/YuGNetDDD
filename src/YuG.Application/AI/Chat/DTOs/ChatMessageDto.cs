using YuG.Domain.AI.ValueObjects;

namespace YuG.Application.AI.Chat.DTOs;

/// <summary>聊天消息 DTO，用于在 Application 层传递消息历史。</summary>
public record ChatMessageDto
{
    /// <summary>消息角色（system/user/assistant/tool）。</summary>
    public string Role { get; init; }

    /// <summary>消息内容。</summary>
    public string Content { get; init; }

    /// <summary>工具调用列表（仅 assistant 角色）。</summary>
    public List<ToolCallItemDto>? ToolCalls { get; init; }

    /// <summary>工具调用 ID（仅 tool 角色）。</summary>
    public string? ToolCallId { get; init; }

    /// <summary>从领域对象构造。</summary>
    public static ChatMessageDto FromDomain(AiChatMessage msg)
    {
        List<ToolCallItemDto>? toolCalls = null;
        if (!string.IsNullOrEmpty(msg.ToolCalls))
        {
            toolCalls = System.Text.Json.JsonSerializer.Deserialize<List<ToolCallItemDto>>(msg.ToolCalls);
        }

        return new ChatMessageDto
        {
            Role = msg.Role,
            Content = msg.Content,
            ToolCalls = toolCalls,
            ToolCallId = msg.ToolCallId
        };
    }

    /// <summary>用于 ORM / 序列化。</summary>
    private ChatMessageDto()
    {
        Role = string.Empty;
        Content = string.Empty;
    }

    /// <summary>创建简单文本消息。</summary>
    public ChatMessageDto(string role, string content)
    {
        Role = role;
        Content = content;
    }

    /// <summary>创建带工具调用的 assistant 消息。</summary>
    public ChatMessageDto(string role, string content, List<ToolCallItemDto>? toolCalls)
    {
        Role = role;
        Content = content;
        ToolCalls = toolCalls;
    }
}

/// <summary>工具调用项 DTO。</summary>
public record ToolCallItemDto
{
    /// <summary>工具调用 ID。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>函数名称。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>函数参数 JSON。</summary>
    public string Arguments { get; init; } = string.Empty;
}
