using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Common;
using YuG.Domain.Identity.Repositories;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.User.RemoveRole;

/// <summary>
/// 移除用户角色命令处理器
/// </summary>
public class Handler : IRequestHandler<RemoveUserRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化移除用户角色命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理移除用户角色命令
    /// </summary>
    /// <param name="request">移除用户角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task Handle(RemoveUserRoleCommand request, CancellationToken cancellationToken)
    {
        // 获取用户（包含角色导航）
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(UserEntity), request.UserId);
        }

        // 获取角色
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new DomainException($"角色不存在：{request.RoleId}");
        }

        // 系统内置角色不允许移除
        if (role.IsSystem)
        {
            throw new DomainException("系统内置角色不允许移除");
        }

        // 移除用户角色
        user.RemoveRole(role);

        // 保存
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
