using FluentValidation;
using YuG.Application.AI.Chat.Common;
using YuG.Application.Common;

namespace YuG.Application.AI.Chat.Send;

/// <summary>发送聊天消息命令。</summary>
public class ChatCommand : CommandBase<ChatReplyResult>
{
    /// <summary>用户输入消息内容。</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>会话 ID。为空则服务端自动创建新会话。</summary>
    public string? SessionId { get; init; }
}

/// <summary>聊天消息命令验证器。</summary>
public class ChatCommandValidator : AbstractValidator<ChatCommand>
{
    /// <summary>初始化验证规则。</summary>
    public ChatCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("消息内容不能为空")
            .MaximumLength(10000).WithMessage("消息内容长度不能超过 10000 个字符");
    }
}
