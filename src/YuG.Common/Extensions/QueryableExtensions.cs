using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace YuG.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IQueryable<T> OrderByIf<T, TKey>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
    {
        return condition ? source.OrderBy(keySelector) : source;
    }

    public static async Task<PageResult<T>> ToPageResultAsync<T>(
        this IQueryable<T> source,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNegativeOrZero(pageSize, nameof(pageSize));
        Guard.AgainstNegativeOrZero(pageIndex, nameof(pageIndex));

        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PageResult<T>
        {
            Items = items,
            TotalCount = count,
            Page = pageIndex,
            PageSize = pageSize
        };
    }
}
