using MediatR;
using YuG.Common.Models;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;

namespace YuG.Application.Permission.Resource.GetList;

/// <summary>获取资源列表查询处理器。</summary>
public class Handler : IRequestHandler<GetResourceListQuery, PageResult<ResourceListItem>>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>初始化处理器。</summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <inheritdoc />
    public async Task<PageResult<ResourceListItem>> Handle(GetResourceListQuery query, CancellationToken cancellationToken)
    {
        ResourceType? type = !string.IsNullOrEmpty(query.Type)
            ? Enum.Parse<ResourceType>(query.Type, ignoreCase: true)
            : null;

        ResourceHttpMethod? httpMethod = !string.IsNullOrEmpty(query.HttpMethod)
            ? Enum.Parse<ResourceHttpMethod>(query.HttpMethod, ignoreCase: true)
            : null;

        var pageResult = await _resourceRepository.GetResourcesPagedAsync(
            query.Page, query.PageSize, type, httpMethod, query.ParentId, query.Status, cancellationToken);

        var items = pageResult.Items.Select(r => new ResourceListItem
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            Type = r.Type.ToString(),
            HttpMethod = r.HttpMethod!.ToString(),
            Path = r.Path,
            Icon = r.Icon,
            Route = r.Route,
            IsHidden = r.IsHidden,
            Badge = r.Badge,
            PermissionCode = r.PermissionCode,
            ParentId = r.ParentId,
            SortOrder = r.SortOrder,
            Status = r.Status.ToString(),
        }).ToList();

        return new PageResult<ResourceListItem>
        {
            Items = items,
            TotalCount = pageResult.TotalCount,
            Page = pageResult.Page,
            PageSize = pageResult.PageSize,
        };
    }
}
