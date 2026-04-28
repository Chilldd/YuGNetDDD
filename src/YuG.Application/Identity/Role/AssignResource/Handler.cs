using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Common;
using YuG.Domain.Identity.Repositories;
using YuG.Domain.Permission.Repositories;
using RoleResult = YuG.Application.Identity.Role.Create.RoleResult;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.AssignResource;

/// <summary>
/// 给角色分配资源命令处理器
/// </summary>
public class Handler : IRequestHandler<AssignResourceCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化给角色分配资源命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IRoleRepository roleRepository, IResourceRepository resourceRepository)
    {
        _roleRepository = roleRepository;
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理给角色分配资源命令
    /// </summary>
    /// <param name="request">分配资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色响应</returns>
    public async Task<RoleResult> Handle(AssignResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取角色（含资源导航）
        var role = await _roleRepository.GetByIdWithResourcesAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.RoleId);
        }

        // 获取要分配的资源
        var resources = await _resourceRepository.GetByIdsAsync(request.ResourceIds, cancellationToken);

        // 检查资源是否存在
        var notFoundIds = request.ResourceIds.Except(resources.Select(r => r.Id)).ToList();
        if (notFoundIds.Count != 0)
        {
            throw new DomainException($"以下资源不存在：{string.Join(", ", notFoundIds)}");
        }

        // 清除旧资源，分配新资源
        role.ClearResources();
        foreach (var resource in resources)
        {
            role.AssignResource(resource);
        }

        // 保存
        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        return Create.Handler.MapToResult(role);
    }
}
