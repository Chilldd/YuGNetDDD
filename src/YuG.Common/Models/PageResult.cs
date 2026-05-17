namespace YuG.Common.Models;

/// <summary>
/// 通用分页结果
/// </summary>
/// <typeparam name="T">列表项类型</typeparam>
public record PageResult<T>
{
    /// <summary>
    /// 当前页数据
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;
}
