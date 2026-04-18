using MediatR;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Domain.Exceptions;
using YuG.Domain.Repositories;
using YuG.Domain.ValueObjects;

namespace YuG.Application.Commands.Resource.Update;

/// <summary>
/// 更新资源命令处理器
/// </summary>
public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, ResourceResponse>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化更新资源命令处理器
    /// </summaryparam>
    /// <param name="resourceRepository">资源仓储</param>
    public UpdateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理更新资源命令
    /// </summary>
    /// <param name="request">更新资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResponse> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取资源
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 检查编码是否与其他资源冲突
        var existingResource = await _resourceRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingResource != null && existingResource.Id != request.Id)
        {
            throw new DomainException($"资源编码 '{request.Code}' 已被其他资源使用");
        }

        // 解析 HTTP 方法
        var httpMethod = ResourceHttpMethod.FromString(request.HttpMethod);

        // 解析状态
        var status = string.IsNullOrEmpty(request.Status) ? ResourceStatus.Active : ResourceStatus.FromString(request.Status);

        // 更新资源
        resource.Update(
            request.Name,
            request.Code,
            request.Description,
            httpMethod,
            request.Path,
            request.ParentId,
            request.SortOrder,
            status);

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
