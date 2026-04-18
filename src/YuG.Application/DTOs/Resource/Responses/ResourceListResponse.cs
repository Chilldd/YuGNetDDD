namespace YuG.Application.DTOs.Resource.Responses;

/// <summary>
/// 资源列表响应
/// </summary>
public record ResourceListResponse
{
    /// <summary>
    /// 资源列表
    /// </summary>
    public IReadOnlyList<ResourceResponse> Resources { get; init; } = [];

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; init; }
}
