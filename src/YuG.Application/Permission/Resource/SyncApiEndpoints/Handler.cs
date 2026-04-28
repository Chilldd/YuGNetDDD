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
        // 1. 获取现有 API 资源（只处理 API 类型）
        var existingResources = await _resourceRepository.GetAllAsync(cancellationToken);
        var apiResources = existingResources.Where(r => r.Type == ResourceType.Api).ToList();
        var existingDict = apiResources
            .ToDictionary(r => (r.Path!.ToLowerInvariant(), r.HttpMethod!.Value), r => r);
        var existingByCodeDict = existingResources
            .ToDictionary(r => r.Code.ToLowerInvariant(), r => r);

        // 2. 处理控制器（作为父资源）
        var controllerMapping = new Dictionary<string, long>();
        int addedCount = 0;
        int updatedCount = 0;
        var sortOrder = 1;

        foreach (var controller in request.Controllers)
        {
            var code = controller.GeneratedCode.ToLowerInvariant();

            if (!existingByCodeDict.TryGetValue(code, out var existingResource))
            {
                // 新增控制器资源（Api 类型）
                var resource = new ResourceEntity(
                    name: controller.DisplayName,
                    code: controller.GeneratedCode,
                    type: ResourceType.Api,
                    description: controller.Description,
                    parentId: null,
                    sortOrder: sortOrder++,
                    status: ResourceStatus.Active);
                resource.ChangeEndpoint(GenerateControllerPath(controller.ControllerName), ResourceHttpMethod.Get);

                await _resourceRepository.AddAsync(resource, cancellationToken);
                controllerMapping.Add(controller.ControllerName, resource.Id);
                addedCount++;
            }
            else
            {
                // 检查是否需要更新
                var needUpdate = false;

                if (existingResource.Name != controller.DisplayName)
                {
                    existingResource.Rename(controller.DisplayName);
                    needUpdate = true;
                }

                if (existingResource.Description != (controller.Description ?? string.Empty))
                {
                    existingResource.ChangeDescription(controller.Description);
                    needUpdate = true;
                }

                if (needUpdate)
                {
                    _resourceRepository.Update(existingResource);
                    updatedCount++;
                }

                controllerMapping.Add(controller.ControllerName, existingResource.Id);
            }
        }

        // 3. 处理端点（作为子资源）
        foreach (var endpoint in request.Endpoints)
        {
            var normalizedPath = endpoint.Path.ToLowerInvariant();
            var key = (normalizedPath, endpoint.HttpMethod);

            // 获取父级控制器 ID
            controllerMapping.TryGetValue(endpoint.ControllerName, out var parentId);

            if (!existingDict.TryGetValue(key, out var existingResource))
            {
                // 新增端点资源（Api 类型）
                var resource = new ResourceEntity(
                    name: endpoint.DisplayName,
                    code: endpoint.GeneratedCode,
                    type: ResourceType.Api,
                    description: endpoint.Description,
                    parentId: parentId,
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

                if (existingResource.ParentId != parentId)
                {
                    existingResource.MoveTo(parentId);
                    needUpdate = true;
                }

                if (needUpdate)
                {
                    _resourceRepository.Update(existingResource);
                    updatedCount++;
                }
            }
        }

        // 4. 保存变更
        await _resourceRepository.SaveChangesAsync(cancellationToken);

        // 5. 返回统计结果
        return new SyncApiEndpointsResult
        {
            AddedCount = addedCount,
            UpdatedCount = updatedCount,
            TotalEndpoints = request.Controllers.Count + request.Endpoints.Count
        };
    }

    /// <summary>
    /// 生成控制器路径
    /// </summary>
    private static string GenerateControllerPath(string controllerName)
    {
        // 移除 Controller 后缀，转为 kebab-case 路径
        var name = controllerName.Replace("Controller", string.Empty, StringComparison.Ordinal);
        return $"/api/{name}".ToLowerInvariant();
    }
}
