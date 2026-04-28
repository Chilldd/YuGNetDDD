using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Identity.Repositories;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.Delete;

/// <summary>
/// 删除角色命令处理器
/// </summary>
public class Handler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化删除角色命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理删除角色命令
    /// </summary>
    /// <param name="request">删除角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.Id);
        }

        _roleRepository.Delete(role);
        await _roleRepository.SaveChangesAsync(cancellationToken);
    }
}
