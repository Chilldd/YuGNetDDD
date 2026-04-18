using MediatR;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Domain.Entities;
using YuG.Domain.Exceptions;
using YuG.Domain.Repositories;
using YuG.Domain.ValueObjects;

namespace YuG.Application.Resource.Commands.Create;

/// <summary>
/// 创建资源命令处理器
/// </summary>
public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, ResourceResponse>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化创建资源命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public CreateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理创建资源命令
    /// </summary>
    /// <param name="request">创建资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResponse> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        // 检查编码是否已存在
        if (await _resourceRepository.CodeExistsAsync(request.Code, cancellationToken))
        {
            throw new DomainException($"资源编码 '{request.Code}' 已存在");
        }

        // 解析 HTTP 方法
        var httpMethod = ResourceHttpMethod.FromString(request.HttpMethod);

        // 解析状态
        var status = string.IsNullOrEmpty(request.Status) ? ResourceStatus.Active : ResourceStatus.FromString(request.Status);

        // 创建资源
        var resource = new Domain.Entities.Resource(
            request.Name,
            request.Code,
            request.Description,
            httpMethod,
            request.Path,
            request.ParentId,
            request.SortOrder,
            status);

        // 保存到数据库
        await _resourceRepository.AddAsync(resource, cancellationToken);
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
