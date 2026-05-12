using MediatR;
using YuG.Domain.Common;
using YuG.Domain.Permission.Enums;
using YuG.Domain.Permission.Repositories;
using ResourceEntity = YuG.Domain.Permission.Entities.Resource;

namespace YuG.Application.Permission.Resource.SyncApiEndpoints;

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
        var existingDict = existingResources
            .Where(r => r.Type == ResourceType.Api)
            .ToDictionary(r => (r.Path!.ToLowerInvariant(), r.HttpMethod!.Value), r => r);

        int addedCount = 0;
        int updatedCount = 0;
        var sortOrder = 1;

        // 2. 处理端点（平铺，不含父级分组）
        foreach (var endpoint in request.Endpoints)
        {
            var normalizedPath = endpoint.Path.ToLowerInvariant();
            var key = (normalizedPath, endpoint.HttpMethod);

            if (!existingDict.TryGetValue(key, out var existingResource))
            {
                // 新增 API 资源
                var resource = new ResourceEntity(
                    name: endpoint.DisplayName,
                    code: endpoint.GeneratedCode,
                    type: ResourceType.Api,
                    description: endpoint.Description,
                    parentId: null,
                    sortOrder: sortOrder++,
                    status: ResourceStatus.Active);
                resource.ChangeEndpoint(endpoint.Path, endpoint.HttpMethod);

                await _resourceRepository.AddAsync(resource, cancellationToken);
                addedCount++;
            }
            else
            {
                // 检查是否需要更新
                var needUpdate = false;

                if (existingResource.Name != endpoint.DisplayName)
                {
                    existingResource.Rename(endpoint.DisplayName);
                    needUpdate = true;
                }

                if (existingResource.Code != endpoint.GeneratedCode)
                {
                    existingResource.ChangeCode(endpoint.GeneratedCode);
                    needUpdate = true;
                }

                if (existingResource.Path != endpoint.Path ||
                    existingResource.HttpMethod != endpoint.HttpMethod)
                {
                    existingResource.ChangeEndpoint(endpoint.Path, endpoint.HttpMethod);
                    needUpdate = true;
                }

                if (existingResource.Description != endpoint.Description)
                {
                    existingResource.ChangeDescription(endpoint.Description);
                    needUpdate = true;
                }

                if (needUpdate)
                {
                    _resourceRepository.Update(existingResource);
                    updatedCount++;
                }
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
}
