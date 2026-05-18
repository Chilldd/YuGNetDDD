using System.Data;
using Dapper;
using YuG.Common.Models;

namespace YuG.Application.Common.Queries;

/// <summary>
/// 查询通用扩展方法
/// </summary>
public static class QueryHelpers
{
    /// <summary>
    /// 执行分页查询，自动处理 COUNT + LIMIT/OFFSET + PageResult 包装
    /// </summary>
    /// <param name="connection">数据库连接</param>
    /// <param name="sqlCount">COUNT 查询 SQL（不含 ORDER BY）</param>
    /// <param name="sqlData">数据查询 SQL（需包含 LIMIT @PageSize OFFSET @Offset 占位符）</param>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="parameters">查询参数（可选）</param>
    public static async Task<PageResult<T>> ToPageResultAsync<T>(
        this IDbConnection connection,
        string sqlCount,
        string sqlData,
        int page,
        int pageSize,
        object? parameters = null)
    {
        var totalCount = await connection.ExecuteScalarAsync<int>(sqlCount, parameters);

        var offset = (page - 1) * pageSize;
        var p = new DynamicParameters(parameters);
        p.Add("PageSize", pageSize);
        p.Add("Offset", offset);

        var items = await connection.QueryAsync<T>(sqlData, p);

        return new PageResult<T>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
