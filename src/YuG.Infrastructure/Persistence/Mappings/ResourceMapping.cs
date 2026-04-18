using YuG.Domain.Entities;
using YuG.Domain.ValueObjects;
using YuG.Infrastructure.Persistence.Entities;

namespace YuG.Infrastructure.Persistence.Mappings;

/// <summary>
/// 资源 ORM 实体与领域实体的映射转换器
/// </summary>
public static class ResourceMapping
{
    /// <summary>
    /// 将 ORM ORM 实体映射到领域实体
    /// </summary>
    /// <param name="entity">ORM 实体</param>
    /// <returns>领域实体</returns>
    public static Resource ToDomain(this ResourceEntity entity)
    {
        var resource = new Resource(
            entity.Name,
            entity.Code,
            entity.Description,
            ResourceHttpMethod.FromString(entity.HttpMethod),
            entity.Path,
            entity.ParentId,
            entity.SortOrder,
            ResourceStatus.FromString(entity.Status))
        {
            Id = entity.Id
        };

        return resource;
    }

    /// <summary>
    /// 将领域实体映射到 ORM 实体
    /// </summary>
    /// <param name="resource">领域实体</param>
    /// <returns>ORM 实体</returns>
    public static ResourceEntity ToEntity(this Resource resource)
    {
        var entity = new ResourceEntity
        {
            Id = resource.Id,
            Name = resource.Name,
            Code = resource.Code,
            Description = resource.Description,
            HttpMethod = resource.HttpMethod.ToString(),
            Path = resource.Path,
            ParentId = resource.ParentId,
            SortOrder = resource.SortOrder,
            Status = resource.Status.ToString()
            // CreatedAt 和 UpdatedAt 由 EF Core 自动处理
        };

        return entity;
    }
}
