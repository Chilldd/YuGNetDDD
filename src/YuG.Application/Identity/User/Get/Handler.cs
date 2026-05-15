using MediatR;
using YuG.Domain.Identity.Repositories;

namespace YuG.Application.Identity.User.Get;

/// <summary>
/// 获取单个用户查询处理器
/// </summary>
public class Handler : IRequestHandler<GetUserQuery, GetUserResult?>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化获取单个用户查询处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    public Handler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理获取单个用户查询
    /// </summary>
    /// <param name="query">获取单个用户查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户查询结果，不存在则返回 null</returns>
    public async Task<GetUserResult?> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(query.Id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return new GetUserResult
        {
            Id = user.Id,
            Username = user.Username,
            Status = user.Status.ToString(),
            RoleIds = user.Roles.Select(r => r.Id).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
