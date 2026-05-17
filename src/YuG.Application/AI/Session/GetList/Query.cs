using MediatR;

namespace YuG.Application.AI.Session.GetList;

/// <summary>获取当前用户的会话列表。</summary>
public class GetSessionListQuery : IRequest<GetSessionListResult>
{
}
