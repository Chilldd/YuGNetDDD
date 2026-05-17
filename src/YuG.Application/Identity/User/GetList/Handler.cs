using MediatR;
using Microsoft.EntityFrameworkCore;
using YuG.Common.Extensions;
using YuG.Domain.Identity.Repositories;

namespace YuG.Application.Identity.User.GetList;

/// <summary>
/// 获取用户列表查询处理器
/// </summary>
public class Handler : IRequestHandler<GetUserListQuery, GetUserListResult>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化获取用户列表查询处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    public Handler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理获取用户列表查询
    /// </summary>
    /// <param name="query">获取用户列表查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户列表结果</returns>
    public async Task<GetUserListResult> Handle(GetUserListQuery query, CancellationToken cancellationToken)
    {
        var pageResult = await _userRepository.GetQueryable()
            .Select(u => new UserListItem
            {
                Id = u.Id,
                Username = u.Username,
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToPageResultAsync(query.PageIndex, query.PageSize, cancellationToken);

        return new GetUserListResult
        {
            Items = pageResult.Items,
            TotalCount = pageResult.TotalCount,
            Page = pageResult.Page,
            PageSize = pageResult.PageSize
        };
    }
}
