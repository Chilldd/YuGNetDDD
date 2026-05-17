using YuG.Application.Common;

namespace YuG.Application.AI.Session.List;

/// <summary>获取当前用户的会话列表。</summary>
public class SessionListQuery : CommandBase<IReadOnlyList<SessionListItemResult>>
{
}
