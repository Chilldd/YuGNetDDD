using MediatR;
using YuG.Domain.Common;
using YuG.Domain.Permission.Repositories;

namespace YuG.Application.Permission.Resource.Activate;

/// <summary>
/// 激活资源命令处理器
/// </summary>
public class Handler : IRequestHandler<ActivateResourceCommand, ResourceResult>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化激活资源命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理激活资源命令
    /// </summary>
    /// <param name="request">激活资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResult> Handle(ActivateResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取资源
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 激活资源
        resource.Activate();

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
