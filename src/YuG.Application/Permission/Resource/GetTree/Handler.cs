using MediatR;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;

namespace YuG.Application.Permission.Resource.GetTree;

/// <summary>
/// 获取资源树查询处理器
/// </summary>
public class Handler : IRequestHandler<GetResourceTreeQuery, GetResourceTreeResult>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化获取资源树查询处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理获取资源树查询
    /// </summary>
    /// <param name="query">获取资源树查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源树结果</returns>
    public async Task<GetResourceTreeResult> Handle(GetResourceTreeQuery query, CancellationToken cancellationToken)
    {
        var resources = await _resourceRepository.GetAllAsync(cancellationToken);

        // 应用筛选
        ResourceType? filterType = null;
        var filtered = resources.AsEnumerable();

        if (!string.IsNullOrEmpty(query.Type))
        {
            filterType = Enum.Parse<ResourceType>(query.Type, ignoreCase: true);
            filtered = filtered.Where(r => r.Type == filterType.Value);
        }

        if (query.Status.HasValue)
        {
            filtered = filtered.Where(r => r.Status == query.Status.Value);
        }

        var list = filtered.ToList();

        // 按子类型（Page/Api）筛选时，包含祖先节点以保持树形结构
        if (filterType.HasValue && filterType.Value != ResourceType.Menu)
        {
            var includedIds = list.Select(r => r.Id).ToHashSet();
            var pendingIds = list
                .Where(r => r.ParentId.HasValue)
                .Select(r => r.ParentId!.Value)
                .Distinct()
                .Where(id => !includedIds.Contains(id))
                .ToHashSet();

            while (pendingIds.Count > 0)
            {
                var ancestors = resources.Where(r => pendingIds.Remove(r.Id)).ToList();
                list.AddRange(ancestors);
                foreach (var a in ancestors) includedIds.Add(a.Id);

                pendingIds = ancestors
                    .Where(r => r.ParentId.HasValue && !includedIds.Contains(r.ParentId.Value))
                    .Select(r => r.ParentId!.Value)
                    .ToHashSet();
            }
        }

        var treeItems = list.Select(r => new ResourceTreeItem
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            Type = r.Type.ToString(),
            HttpMethod = r.HttpMethod?.ToString(),
            Path = r.Path,
            Icon = r.Icon,
            Route = r.Route,
            IsHidden = r.IsHidden,
            Badge = r.Badge,
            PermissionCode = r.PermissionCode,
            ParentId = r.ParentId,
            SortOrder = r.SortOrder,
            Status = r.Status.ToString()
        }).ToList();

        // 构建完整树形结构
        var allRoots = BuildTree(treeItems, null);

        // 分离 Menu 根节点和孤立资源（无父级的非 Menu 类型）
        var menuRoots = allRoots.Where(x => x.Type == nameof(ResourceType.Menu))
            .OrderBy(x => x.SortOrder)
            .ToList();

        var orphanItems = allRoots.Where(x => x.Type != nameof(ResourceType.Menu))
            .OrderBy(x => x.SortOrder)
            .ToList();

        // 如果有孤立资源，创建虚拟的"其他"节点统一收纳
        if (orphanItems.Count > 0)
        {
            menuRoots.Add(new ResourceTreeItem
            {
                Id = -1,
                Name = "其他",
                Code = "_other_",
                Type = nameof(ResourceType.Menu),
                Description = "未分组的资源（无父级）",
                Children = orphanItems
            });
        }

        return new GetResourceTreeResult
        {
            Items = menuRoots
        };
    }

    /// <summary>
    /// 递归构建资源树
    /// </summary>
    /// <param name="allItems">所有资源节点</param>
    /// <param name="parentId">父级标识</param>
    /// <returns>子树节点列表</returns>
    private static List<ResourceTreeItem> BuildTree(List<ResourceTreeItem> allItems, long? parentId)
    {
        return allItems
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.SortOrder)
            .Select(x =>
            {
                x.Children = BuildTree(allItems, x.Id);
                return x;
            })
            .ToList();
    }
}
