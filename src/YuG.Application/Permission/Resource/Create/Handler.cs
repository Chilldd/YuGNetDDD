using MediatR;
using YuG.Domain.Common;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Permission.Resource.Create;

/// <summary>
/// 创建资源命令处理器
/// </summary>
public class Handler : IRequestHandler<CreateResourceCommand, ResourceResult>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化创建资源命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理创建资源命令
    /// </summary>
    /// <param name="request">创建资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>资源响应</returns>
    public async Task<ResourceResult> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        // 检查编码是否已存在
        if (await _resourceRepository.CodeExistsAsync(request.Code, cancellationToken))
        {
            throw new DomainException($"资源编码 '{request.Code}' 已存在");
        }

        // 解析资源类型
        var type = Enum.Parse<ResourceType>(request.Type, ignoreCase: true);

        // 解析状态
        var status = string.IsNullOrEmpty(request.Status) ? ResourceStatus.Active : Enum.Parse<ResourceStatus>(request.Status, ignoreCase: true);

        // 创建资源（共用基础字段）
        var resource = new ResourceEntity(
            request.Name,
            request.Code,
            type,
            request.Description,
            request.ParentId,
            request.SortOrder,
            status);

        // 根据类型配置特有信息
        switch (type)
        {
            case ResourceType.Api:
                var httpMethod = Enum.Parse<ResourceHttpMethod>(request.HttpMethod!, ignoreCase: true);
                resource.ChangeEndpoint(request.Path!, httpMethod);
                break;

            case ResourceType.Menu:
                resource.ConfigureMenu(
                    request.Icon,
                    request.Route,
                    request.Component,
                    request.IsHidden,
                    request.Badge);
                break;

            case ResourceType.Button:
                resource.ConfigureButton(request.PermissionCode);
                break;
        }

        // 保存到数据库
        await _resourceRepository.AddAsync(resource, cancellationToken);
        await _resourceRepository.SaveChangesAsync(cancellationToken);

        // 返回响应
        return MapToResult(resource);
    }

    /// <summary>
    /// 将资源实体映射为响应结果
    /// </summary>
    internal static ResourceResult MapToResult(ResourceEntity resource)
    {
        return new ResourceResult
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
