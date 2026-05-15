using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Guards;
using YuG.Domain.Common;
using YuG.Domain.Identity.Repositories;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.AssignUsers;

/// <summary>
/// 给角色分配用户命令处理器
/// </summary>
public class Handler : IRequestHandler<AssignRoleUsersCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化给角色分配用户命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    /// <param name="userRepository">用户仓储</param>
    public Handler(IRoleRepository roleRepository, IUserRepository userRepository)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理给角色分配用户命令
    /// </summary>
    /// <param name="request">分配用户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task Handle(AssignRoleUsersCommand request, CancellationToken cancellationToken)
    {
        // 获取角色（含用户导航）
        var role = await _roleRepository.GetByIdWithUsersAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.RoleId);
        }

        SystemRoleGuard.AgainstModification(role);

        // 获取要分配的用户
        var users = await _userRepository.FindAsync(u => request.UserIds.Contains(u.Id), cancellationToken);

        // 检查用户是否存在
        var foundIds = users.Select(u => u.Id).ToHashSet();
        var notFoundIds = request.UserIds.Where(id => !foundIds.Contains(id)).ToList();
        if (notFoundIds.Count != 0)
        {
            throw new DomainException($"以下用户不存在：{string.Join(", ", notFoundIds)}");
        }

        // 分配用户（追加模式，跳过已有）
        role.AssignUsers(users);

        // 保存
        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(cancellationToken);
    }
}
