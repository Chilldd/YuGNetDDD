using MediatR;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Domain.Entities;
using YuG.Domain.Repositories;

namespace YuG.Application.Queries.Resource.GetAll;

/// <summary>
/// 获取所有资源查询处理器
/// </summary>
public class GetAllResourcesQueryHandler : IRequestHandler<GetAllResourcesQuery, ResourceListResponse>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化获取所有资源查询查询器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public GetAllResourcesQueryHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理获取所有资源查询
    /// </summary>
    /// <param name="request">获取所有资源查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源列表响应</returns>
    public async Task<ResourceListResponse> Handle(GetAllResourcesQuery request, CancellationToken cancellationToken)
    {
        // 根据查询条件获取资源
        IReadOnlyList<Domain.Entities.Resource> resources;

        if (request.ActiveOnly == true)
        {
            // 只获取激活状态
            resources = await _resourceRepository.GetActiveAsync(cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.HttpMethod))
        {
            // 根据 HTTP 方法获取
            resources = await _resourceRepository.GetByHttpMethodAsync(request.HttpMethod, cancellationToken);
        }
        else if (request.ParentId.HasValue)
        {
            // 根据父级资源标识获取
            resources = await _resourceRepository.GetByParentIdAsync(request.ParentId.Value, cancellationToken);
        }
        else
        {
            // 获取所有资源
            resources = await _resourceRepository.GetAllAsync(cancellationToken);
        }

        // 转换为响应
        var resourceResponses = resources.Select(r => new ResourceResponse
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            HttpMethod = r.HttpMethod.ToString(),
            Path = r.Path,
            ParentId = r.ParentId,
            SortOrder = r.SortOrder,
            Status = r.Status.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        return new ResourceListResponse
        {
            Resources = resourceResponses,
            TotalCount = resourceResponses.Count
        };
    }
}
