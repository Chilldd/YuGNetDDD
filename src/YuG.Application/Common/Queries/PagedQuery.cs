using MediatR;

namespace YuG.Application.Common.Queries;

/// <summary>
/// 分页查询基类，所有分页查询应继承此类
/// </summary>
/// <typeparam name="TResponse">查询响应类型</typeparam>
public abstract class PagedQuery<TResponse> : IRequest<TResponse>
{
    /// <summary>当前页码，从 1 开始。</summary>
    public int Page { get; init; } = 1;

    /// <summary>每页条数，默认 10。</summary>
    public int PageSize { get; init; } = 10;
}
