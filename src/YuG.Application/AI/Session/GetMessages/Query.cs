using FluentValidation;
using YuG.Application.Common;
using YuG.Common.Models;

namespace YuG.Application.AI.Session.GetMessages;

/// <summary>获取会话消息历史查询。</summary>
public class GetSessionMessagesQuery : PagedQuery<PageResult<MessageItem>>
{
    /// <summary>会话 ID。</summary>
    public string SessionId { get; init; } = string.Empty;
}

/// <summary>验证器。</summary>
public class GetSessionMessagesQueryValidator : AbstractValidator<GetSessionMessagesQuery>
{
    /// <summary>初始化验证规则。</summary>
    public GetSessionMessagesQueryValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("会话 ID 不能为空");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("页码必须大于等于 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("每页条数必须在 1-100 之间");
    }
}
