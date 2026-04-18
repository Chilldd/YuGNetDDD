using MediatR;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Domain.Exceptions;
using YuG.Domain.Repositories;

namespace YuG.Application.Queries.Resource.GetById;

/// <summary>
/// 获取资源查询处理器
/// </summary>
public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, ResourceResponse>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化获取资源查询处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public GetResourceByIdQueryHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理获取资源查询
    /// </summary>
    /// <param name="request">获取资源查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResponse> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        // 获取资源
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 返回响应
        return new ResourceResponse
        {
            Id = resource.Id,
            Name = resource.Name,
            Code = resource.Code,
            Description = resource.Description,
            HttpMethod = resource.HttpMethod.ToString(),
            Path = resource.Path,
            ParentId = resource.ParentId,
            SortOrder = resource.SortOrder,
            Status = resource.Status.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
