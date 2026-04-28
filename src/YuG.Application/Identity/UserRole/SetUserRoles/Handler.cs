using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Common;
using YuG.Domain.Identity.Repositories;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.UserRole.SetUserRoles;

/// <summary>
/// 设置用户角色命令处理器
/// </summary>
public class Handler : IRequestHandler<SetUserRolesCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化设置用户角色命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理设置用户角色命令
    /// </summary>
    /// <param name="request">设置用户角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        // 获取用户
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(UserEntity), request.UserId);
        }

        // 获取要分配的角色
        var roles = await _roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);

        // 检查角色是否存在
        var notFoundIds = request.RoleIds.Except(roles.Select(r => r.Id)).ToList();
        if (notFoundIds.Count != 0)
        {
            throw new DomainException($"以下角色不存在：{string.Join(", ", notFoundIds)}");
        }

        // 设置用户角色（覆盖模式）
        user.SetRoles(roles);

        // 保存
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
