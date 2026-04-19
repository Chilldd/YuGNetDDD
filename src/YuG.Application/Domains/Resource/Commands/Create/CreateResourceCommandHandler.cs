using MediatR;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Domain.Common;
using YuG.Domain.Permission.Entities;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Domains.Resource.Commands.Create;

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
        var httpMethod = Enum.Parse<ResourceHttpMethod>(request.HttpMethod, ignoreCase: true);

        // 解析状态
        var status = string.IsNullOrEmpty(request.Status) ? ResourceStatus.Active : Enum.Parse<ResourceStatus>(request.Status, ignoreCase: true);

        // 创建资源
        var resource = new ResourceEntity(
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
