using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Identity.Repositories;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.UnassignResource;

/// <summary>
/// 从角色移除资源命令处理器
/// </summary>
public class Handler : IRequestHandler<UnassignResourceCommand>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化从角色移除资源命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理从角色移除资源命令
    /// </summary>
    /// <param name="request">移除资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task Handle(UnassignResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取角色（含资源导航）
        var role = await _roleRepository.GetByIdWithResourcesAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.RoleId);
        }

        // 查找并移除资源
        var resource = role.Resources.FirstOrDefault(r => r.Id == request.ResourceId);
        if (resource is null)
        {
            throw new NotFoundException(nameof(Domain.Permission.Entities.Resource), request.ResourceId);
        }

        role.UnassignResource(resource);

        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(cancellationToken);
    }
}
