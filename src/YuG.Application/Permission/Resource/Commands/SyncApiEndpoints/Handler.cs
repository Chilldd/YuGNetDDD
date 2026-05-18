using MediatR;
using YuG.Domain.Common;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Permission.Resource.Commands.SyncApiEndpoints;

/// <summary>
/// 同步 API 端点命令处理器
/// </summary>
public class Handler : IRequestHandler<SyncApiEndpointsCommand, SyncApiEndpointsResult>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化同步 API 端点命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public Handler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理同步 API 端点命令
    /// </summary>
    /// <param name="request">同步命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>同步结果统计</returns>
    public async Task<SyncApiEndpointsResult> Handle(
        SyncApiEndpointsCommand request,
        CancellationToken cancellationToken)
    {
        // 1. 获取现有 API 资源
        var existingResources = await _resourceRepository.GetAllAsync(cancellationToken);
        var existingByPath = existingResources
            .Where(r => r.Type == ResourceType.Api && r.HttpMethod.HasValue && r.Path is not null)
            .ToDictionary(r => (r.Path!.ToLowerInvariant(), r.HttpMethod!.Value), r => r);

        // 按 PermissionCode 索引（唯一约束，用于路由变更等冲突检测）
        var existingByPermissionCode = existingResources
            .Where(r => r.Type == ResourceType.Api && r.PermissionCode is not null)
            .ToDictionary(r => r.PermissionCode!, r => r);

        int addedCount = 0;
        int updatedCount = 0;
        var sortOrder = 1;

        // 2. 处理端点（平铺，不含父级分组）
        foreach (var endpoint in request.Endpoints)
        {
            // 跳过无需权限验证的端点
            if (!endpoint.RequirePermission)
            {
                continue;
            }

            var normalizedPath = endpoint.Path.ToLowerInvariant();
            var key = (normalizedPath, endpoint.HttpMethod);

            if (!existingByPath.TryGetValue(key, out var existingResource))
            {
                // 按路径未匹配到，按 PermissionCode 查找
                // 处理路由变更或同权限多端点场景：PermissionCode 已在其他路径上存在
                if (endpoint.PermissionCode is not null &&
                    existingByPermissionCode.TryGetValue(endpoint.PermissionCode, out var resourceByCode))
                {
                    existingResource = resourceByCode;
                    ApplyEndpointUpdates(existingResource, endpoint, ref updatedCount);
                    _resourceRepository.Update(existingResource);

                    // 更新路径索引，避免同一端点重复处理
                    existingByPath[key] = existingResource;
                }
                else
                {
                    // 真正的新资源
                    var resource = new ResourceEntity(
                        name: endpoint.Description,
                        code: endpoint.GeneratedCode,
                        type: ResourceType.Api,
                        description: endpoint.Description,
                        parentId: null,
                        sortOrder: sortOrder++,
                        status: ResourceStatus.Active);
                    resource.ChangeEndpoint(endpoint.Path, endpoint.HttpMethod);
                    resource.ConfigureApiPermission(endpoint.PermissionCode);

                    await _resourceRepository.AddAsync(resource, cancellationToken);
                    addedCount++;
                }
            }
            else
            {
                ApplyEndpointUpdates(existingResource, endpoint, ref updatedCount);
            }
        }

        // 3. 保存变更
        await _resourceRepository.SaveChangesAsync(cancellationToken);

        // 4. 返回统计结果
        return new SyncApiEndpointsResult
        {
            AddedCount = addedCount,
            UpdatedCount = updatedCount,
            TotalEndpoints = request.Endpoints.Count
        };
    }

    /// <summary>
    /// 对现有资源应用端点变更的差异更新
    /// </summary>
    /// <param name="resource">现有资源</param>
    /// <param name="endpoint">扫描到的端点信息</param>
    /// <param name="updatedCount">更新计数器引用</param>
    private void ApplyEndpointUpdates(
        ResourceEntity resource,
        DiscoveredEndpointInfo endpoint,
        ref int updatedCount)
    {
        var changed = resource.SyncFromEndpoints(
            endpoint.Description,
            endpoint.GeneratedCode,
            endpoint.Description,
            endpoint.Path,
            endpoint.HttpMethod,
            endpoint.PermissionCode);

        if (changed)
        {
            _resourceRepository.Update(resource);
            updatedCount++;
        }
    }
}
