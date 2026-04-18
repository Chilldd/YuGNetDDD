using YuG.Application.Common;
using YuG.Application.DTOs.Resource.Responses;

namespace YuG.Application.Queries.Resource.GetAll;

/// <summary>
/// 获取所有资源查询
/// </summary>
public class GetAllResourcesQuery : QueryBase<ResourceListResponse>
{
    /// <summary>
    /// HTTP 方法筛选（可选）
    /// </summary>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// 父级资源标识筛选（可选）
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// 是否只返回激活状态（可选）
    /// </summary>
    public bool? ActiveOnly { get; init; }
}
