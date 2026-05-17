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

    /// <inheritdoc />
    public async Task<PageResult<UserListItem>> Handle(GetUserListQuery query, CancellationToken cancellationToken)
    {
        var pageResult = await _userRepository.GetUsersPagedAsync(query.Page, query.PageSize, cancellationToken);

        var items = pageResult.Items.Select(u => new UserListItem
        {
            Id = u.Id,
            Username = u.Username,
            Status = u.Status.ToString(),
            CreatedAt = u.CreatedAt,
        }).ToList();

        return new PageResult<UserListItem>
        {
            Items = items,
            TotalCount = pageResult.TotalCount,
            Page = pageResult.Page,
            PageSize = pageResult.PageSize,
        };
    }
}
