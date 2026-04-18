using MediatR;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Domain.Exceptions;
using YuG.Domain.Repositories;

namespace YuG.Application.Commands.Resource.Disable;

/// <summary>
/// 禁用资源命令处理器
/// </summary>
public class DisableResourceCommandHandler : IRequestHandler<DisableResourceCommand, ResourceResponse>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化禁用资源命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public DisableResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理禁用资源命令
    /// </summary>
    /// <param name="request">禁用资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResponse> Handle(DisableResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取资源
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 禁用资源
        resource.Disable();

        // 保存到数据库
        _resourceRepository.Update(resource);
        await _resourceRepository.SaveChangesAsync(cancellationToken);

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
