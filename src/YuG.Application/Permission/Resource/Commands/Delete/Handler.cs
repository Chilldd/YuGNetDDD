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
        if (resource == null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 一次性加载所有资源，构建父子关系映射
        var allResources = await _resourceRepository.GetAllAsync(cancellationToken);
        var childrenMap = allResources
            .Where(r => r.ParentId.HasValue)
            .GroupBy(r => r.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 递归收集所有后代（深度优先，子节点在前）
        var descendantsToDelete = new List<ResourceEntity>();
        CollectDescendants(request.Id, childrenMap, descendantsToDelete);

        // 从叶子到根删除（反向确保子节点先于父节点删除）
        for (var i = descendantsToDelete.Count - 1; i >= 0; i--)
        {
            _resourceRepository.Delete(descendantsToDelete[i]);
        }

        // 删除目标资源
        _resourceRepository.Delete(resource);
        await _resourceRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    /// <summary>
    /// 递归收集所有子资源（深度优先，子节点在结果中按从深到浅排列）
    /// </summary>
    private static void CollectDescendants(
        long parentId,
        Dictionary<long, List<ResourceEntity>> childrenMap,
        List<ResourceEntity> result)
    {
        if (!childrenMap.TryGetValue(parentId, out var children))
        {
            return;
        }

        foreach (var child in children)
        {
            CollectDescendants(child.Id, childrenMap, result);
            result.Add(child);
        }
    }
}
