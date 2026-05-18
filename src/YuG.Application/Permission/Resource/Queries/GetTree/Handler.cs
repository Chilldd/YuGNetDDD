using Dapper;
using MediatR;
using YuG.Application.Common.Interfaces;
using YuG.Domain.Permission.Enums;

namespace YuG.Application.Permission.Resource.Queries.GetTree;

/// <summary>
/// 获取资源树查询处理器
/// </summary>
public class Handler : IRequestHandler<GetResourceTreeQuery, GetResourceTreeResult>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    /// <summary>
    /// 初始化获取资源树查询处理器
    /// </summary>
    /// <param name="connectionFactory">SQL 连接工厂</param>
    public Handler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 处理获取资源树查询
    /// </summary>
    /// <param name="query">获取资源树查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源树结果</returns>
    public async Task<GetResourceTreeResult> Handle(GetResourceTreeQuery query, CancellationToken cancellationToken)
    {
        using var conn = _connectionFactory.CreateConnection();

        var resources = await conn.QueryAsync<ResourceTreeItem>(
            """
            SELECT Id, Name, Code, Description, Type, HttpMethod, Path,
                   Icon, Route, IsHidden, Badge, PermissionCode,
                   ParentId, SortOrder, Status
            FROM Resource
            """);

        // 应用筛选
        ResourceType? filterType = null;
        var filtered = resources.AsEnumerable();

        if (!string.IsNullOrEmpty(query.Type))
        {
            filterType = Enum.Parse<ResourceType>(query.Type, ignoreCase: true);
            filtered = filtered.Where(r => r.Type == filterType.Value.ToString());
        }

        if (query.Status.HasValue)
        {
            filtered = filtered.Where(r => r.Status == query.Status.Value.ToString());
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

        // 构建完整树形结构
        var allRoots = BuildTree(list, null);

        // 分离顶层节点（Menu 和 Page）和孤立 Api（无父级的 Api 类型）
        var topLevelItems = allRoots.Where(x => x.Type != nameof(ResourceType.Api))
            .OrderBy(x => x.SortOrder)
            .ToList();

        var orphanApis = allRoots.Where(x => x.Type == nameof(ResourceType.Api))
            .OrderBy(x => x.SortOrder)
            .ToList();

        // 如果有孤立 Api，创建虚拟的"其他"节点统一收纳
        if (orphanApis.Count > 0)
        {
            topLevelItems.Add(new ResourceTreeItem
            {
                Id = -1,
                Name = "其他",
                Code = "_other_",
                Type = nameof(ResourceType.Menu),
                Description = "未分组的 Api",
                Children = orphanApis
            });
        }

        return new GetResourceTreeResult
        {
            Items = topLevelItems
        };
    }

    /// <summary>
    /// 递归构建资源树
    /// </summary>
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
