using MediatR;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;

namespace YuG.Application.Permission.Resource.GetList;

/// <summary>
/// 获取资源列表查询处理器
/// </summary>
public class Handler : IRequestHandler<GetResourceListQuery, GetResourceListResult>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化获取资源列表查询处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理获取资源列表查询
    /// </summary>
    /// <param name="query">获取资源列表查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表结果</returns>
    public async Task<GetResourceListResult> Handle(GetResourceListQuery query, CancellationToken cancellationToken)
    {
        var resources = await _resourceRepository.GetAllAsync(cancellationToken);

        // 应用筛选
        var filtered = resources.AsEnumerable();

        if (!string.IsNullOrEmpty(query.HttpMethod))
        {
            var method = Enum.Parse<ResourceHttpMethod>(query.HttpMethod, ignoreCase: true);
            filtered = filtered.Where(r => r.HttpMethod == method);
        }

        if (query.ParentId.HasValue)
        {
            filtered = filtered.Where(r => r.ParentId == query.ParentId);
        }

        if (query.ActiveOnly == true)
        {
            filtered = filtered.Where(r => r.Status == ResourceStatus.Active);
        }

        var list = filtered.ToList();
        var totalCount = list.Count;

        var items = list.Select(r => new ResourceListItem
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            HttpMethod = r.HttpMethod.ToString(),
            Path = r.Path,
            ParentId = r.ParentId,
            SortOrder = r.SortOrder,
            Status = r.Status.ToString()
        }).ToList();

        return new GetResourceListResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
