namespace YuG.Application.Permission.Resource.SyncApiEndpoints;

/// <summary>
/// 同步 API 端点结果
/// </summary>
public record SyncApiEndpointsResult
{
    /// <summary>
    /// 新增资源数量
    /// </summary>
    public int AddedCount { get; init; }

    /// <summary>
    /// 更新资源数量
    /// </summary>
    public int UpdatedCount { get; init; }

    /// <summary>
    /// 扫描到的总端点数量
    /// </summary>
    public int TotalEndpoints { get; init; }
}
