using System.Linq.Expressions;

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

    public static PageResult<T> ToPageResult<T>(
        this IQueryable<T> source,
        int pageIndex,
        int pageSize)
    {
        Guard.AgainstNegativeOrZero(pageSize, nameof(pageSize));

        var count = source.Count();
        var items = source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PageResult<T>
        {
            Items = items,
            TotalCount = count,
            Page = pageIndex,
            PageSize = pageSize
        };
    }
}
