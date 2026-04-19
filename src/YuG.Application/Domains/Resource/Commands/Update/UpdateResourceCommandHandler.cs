using MediatR;
using YuG.Application.DTOs.Resource.Responses;
using YuG.Domain.Common;
using YuG.Domain.Permission.Entities;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;

namespace YuG.Application.Domains.Resource.Commands.Update;

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
        var httpMethod = Enum.Parse<ResourceHttpMethod>(request.HttpMethod, ignoreCase: true);

        // 解析状态
        var status = string.IsNullOrEmpty(request.Status) ? ResourceStatus.Active : Enum.Parse<ResourceStatus>(request.Status, ignoreCase: true);

        // 更新资源
        resource.Rename(request.Name);
        resource.ChangeCode(request.Code);
        resource.ChangeDescription(request.Description);
        resource.ChangeEndpoint(request.Path, httpMethod);
        resource.MoveTo(request.ParentId);
        resource.ChangeSortOrder(request.SortOrder);

        // 根据目标状态执行对应业务动作
        if (status == ResourceStatus.Active)
        {
            resource.Activate();
        }
        else
        {
            resource.Disable();
        }

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
