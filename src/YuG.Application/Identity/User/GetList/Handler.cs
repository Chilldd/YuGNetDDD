using MediatR;
using YuG.Common.Models;
using YuG.Domain.Identity.Repositories;

namespace YuG.Application.Identity.User.GetList;

/// <summary>
/// 获取用户列表查询处理器
/// </summary>
public class Handler : IRequestHandler<GetUserListQuery, PageResult<UserListItem>>
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
    public async Task<PageResult<UserListItem>> Handle(GetUserListQuery query, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUsersPagedAsync(query.Page, query.PageSize,
            u => new UserListItem
            {
                Id = u.Id,
                Username = u.Username,
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt,
            }, cancellationToken);
    }
}
