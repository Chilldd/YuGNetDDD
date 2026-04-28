using MediatR;
using YuG.Domain.Permission.Repositories;

namespace YuG.Application.Permission.Resource.Get;

/// <summary>
/// 获取资源查询处理器
/// </summary>
public class Handler : IRequestHandler<GetResourceQuery, GetResourceResult?>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化获取资源查询处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理获取资源查询
    /// </summary>
    /// <param name="query">获取资源查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源结果</returns>
    public async Task<GetResourceResult?> Handle(GetResourceQuery query, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(query.Id, cancellationToken);
        if (resource == null)
        {
            return null;
        }

        return new GetResourceResult
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
            Component = resource.Component,
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
}
