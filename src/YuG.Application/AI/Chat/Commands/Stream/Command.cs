using FluentValidation;
using YuG.Application.AI.Chat.DTOs;
using YuG.Application.Common;

namespace YuG.Application.AI.Chat.Commands.Stream;

/// <summary>流式聊天命令。</summary>
public class StreamChatCommand : CommandBase<IAsyncEnumerable<ChatStreamDeltaResult>>
{
    /// <summary>用户输入消息内容。</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>会话 ID。为空则服务端自动创建新会话。</summary>
    public string? SessionId { get; init; }
}

/// <summary>流式聊天命令验证器。</summary>
public class StreamChatCommandValidator : AbstractValidator<StreamChatCommand>
{
    /// <summary>初始化验证规则。</summary>
    public StreamChatCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("消息内容不能为空")
            .MaximumLength(10000).WithMessage("消息内容长度不能超过 10000 个字符");
    }
}
