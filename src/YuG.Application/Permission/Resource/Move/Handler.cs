using MediatR;
using YuG.Domain.Common;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Permission.Resource.Move;

/// <summary>
/// 移动资源命令处理器
/// </summary>
public class Handler : IRequestHandler<MoveResourceCommand, ResourceResult>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化移动资源命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理移动资源命令
    /// </summary>
    /// <param name="request">移动资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResult> Handle(MoveResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取要移动的资源
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 如果目标父级非空，验证父级存在
        ResourceType? parentType = null;
        if (request.ParentId.HasValue)
        {
            var parent = await _resourceRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent is null)
            {
                throw new DomainException($"目标父级资源 '{request.ParentId.Value}' 不存在");
            }
            parentType = parent.Type;
        }

        // 验证父子类型层级关系
        ResourceEntity.ValidateParentChildType(resource.Type, parentType);

        // 检查循环引用：目标父级不能是当前资源的后代
        if (request.ParentId.HasValue)
        {
            await EnsureNoCircularReferenceAsync(request.Id, request.ParentId.Value, cancellationToken);
        }

        // 执行移动
        resource.MoveTo(request.ParentId);

        // 保存到数据库
        _resourceRepository.Update(resource);
        await _resourceRepository.SaveChangesAsync(cancellationToken);

        // 返回响应
        return new ResourceResult
        {
            Id = resource.Id,
            Name = resource.Name,
            Code = resource.Code,
            Description = resource.Description,
            Type = resource.Type.ToString(),
            HttpMethod = resource.HttpMethod?.ToString(),
            Path = resource.Path,
            Icon = resource.Icon,
            Route = resource.Route,
            IsHidden = resource.IsHidden,
            Badge = resource.Badge,
            PermissionCode = resource.PermissionCode,
            ParentId = resource.ParentId,
            SortOrder = resource.SortOrder,
            Status = resource.Status.ToString(),
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt
        };
    }

    /// <summary>
    /// 确保目标父级不是当前资源的后代（防止循环引用）
    /// </summary>
    private async Task EnsureNoCircularReferenceAsync(long resourceId, long targetParentId, CancellationToken cancellationToken)
    {
        var allResources = await _resourceRepository.GetAllAsync(cancellationToken);
        var childrenMap = allResources
            .Where(r => r.ParentId.HasValue)
            .GroupBy(r => r.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var descendantIds = new HashSet<long>();
        CollectDescendantIds(resourceId, childrenMap, descendantIds);

        if (descendantIds.Contains(targetParentId))
        {
            throw new DomainException("不能将资源移动到自己或自己的子资源下");
        }
    }

    /// <summary>
    /// 递归收集所有后代 ID
    /// </summary>
    private static void CollectDescendantIds(
        long parentId,
        Dictionary<long, List<ResourceEntity>> childrenMap,
        HashSet<long> result)
    {
        if (!childrenMap.TryGetValue(parentId, out var children))
        {
            return;
        }

        foreach (var child in children)
        {
            result.Add(child.Id);
            CollectDescendantIds(child.Id, childrenMap, result);
        }
    }
}
