using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Application.Common.Guards;
using YuG.Domain.Common;
using YuG.Domain.Identity.Repositories;
using YuG.Application.Identity.Role.DTOs;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.Commands.Activate;

/// <summary>
/// 激活角色命令处理器
/// </summary>
public class Handler : IRequestHandler<ActivateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化激活角色命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理激活角色命令
    /// </summary>
    /// <param name="request">激活角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色响应</returns>
    public async Task<RoleResult> Handle(ActivateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.Id);
        }

        SystemRoleGuard.AgainstModification(role);

        role.Activate();

        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        return RoleResult.FromEntity(role);
    }
}
