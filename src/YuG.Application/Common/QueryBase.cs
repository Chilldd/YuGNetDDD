using MediatR;

namespace YuG.Application.Common;

/// <summary>
/// 查询基类，所有读操作查询应继承此类
/// </summary>
/// <typeparam name="TResponse">查询响应类型</typeparam>
public abstract class QueryBase<TResponse> : IRequest<TResponse>
{
}
