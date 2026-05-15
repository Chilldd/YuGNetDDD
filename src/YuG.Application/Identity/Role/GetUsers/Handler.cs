using MediatR;
using YuG.Application.Common.Exceptions;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.GetUsers;

/// <summary>
/// 获取角色关联用户查询处理器
/// </summary>
public class Handler : IRequestHandler<GetRoleUsersQuery, GetRoleUsersResult>
{
    private readonly Domain.Identity.Repositories.IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化获取角色关联用户查询处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(Domain.Identity.Repositories.IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理获取角色关联用户查询
    /// </summary>
    /// <param name="request">查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色关联用户结果</returns>
    public async Task<GetRoleUsersResult> Handle(GetRoleUsersQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithUsersAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.RoleId);
        }

        var items = role.Users
            .OrderBy(u => u.Username)
            .Select(u => new RoleUserItem
            {
                Id = u.Id,
                Username = u.Username,
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToList();

        return new GetRoleUsersResult
        {
            Items = items
        };
    }
}
