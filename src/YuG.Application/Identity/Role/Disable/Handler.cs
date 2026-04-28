using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Identity.Repositories;
using RoleResult = YuG.Application.Identity.Role.Create.RoleResult;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.Disable;

/// <summary>
/// 禁用角色命令处理器
/// </summary>
public class Handler : IRequestHandler<DisableRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化禁用角色命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理禁用角色命令
    /// </summary>
    /// <param name="request">禁用角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色响应</returns>
    public async Task<RoleResult> Handle(DisableRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.Id);
        }

        role.Disable();

        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        return Create.Handler.MapToResult(role);
    }
}
