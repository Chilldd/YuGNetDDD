using MediatR;
using YuG.Domain.Common;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Permission.Resource.Commands.Delete;

/// <summary>
/// 删除资源命令处理器
/// </summary>
public class Handler : IRequestHandler<DeleteResourceCommand, Unit>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化删除资源命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理删除资源命令（递归删除所有子资源）
    /// </summary>
    /// <param name="request">删除资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Unit</returns>
    public async Task<Unit> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取资源
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource is null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 一次性加载所有资源，收集所有后代
        var allResources = await _resourceRepository.GetAllAsync(cancellationToken);
        var descendants = ResourceEntity.GetDescendantsInDeleteOrder(request.Id, allResources);

        // 从叶子到根删除（子节点先于父节点删除）
        foreach (var descendant in descendants)
        {
            _resourceRepository.Delete(descendant);
        }

        // 删除目标资源
        _resourceRepository.Delete(resource);
        await _resourceRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
