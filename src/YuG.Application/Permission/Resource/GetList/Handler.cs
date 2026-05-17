using MediatR;
using Microsoft.EntityFrameworkCore;
using YuG.Common.Extensions;
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
        var queryable = _resourceRepository.GetQueryable();

        if (!string.IsNullOrEmpty(query.Type))
        {
            var type = Enum.Parse<ResourceType>(query.Type, ignoreCase: true);
            queryable = queryable.Where(r => r.Type == type);
        }

        if (!string.IsNullOrEmpty(query.HttpMethod))
        {
            var method = Enum.Parse<ResourceHttpMethod>(query.HttpMethod, ignoreCase: true);
            queryable = queryable.Where(r => r.HttpMethod == method);
        }

        if (query.ParentId.HasValue)
        {
            queryable = queryable.Where(r => r.ParentId == query.ParentId);
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(r => r.Status == query.Status.Value);
        }

        var pageResult = await queryable
            .Select(r => new ResourceListItem
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
                Status = r.Status.ToString()
            })
            .ToPageResultAsync(query.Page, query.PageSize, cancellationToken);

        return new GetResourceListResult
        {
            Items = pageResult.Items,
            TotalCount = pageResult.TotalCount,
            Page = pageResult.Page,
            PageSize = pageResult.PageSize
        };
    }
}
