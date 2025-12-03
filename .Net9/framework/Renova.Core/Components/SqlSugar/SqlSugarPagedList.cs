using SqlSugar;
using System.Linq.Expressions;

namespace Renova.Core;

/// <summary>
/// SqlSugar 分页结果对象
/// </summary>
/// <typeparam name="TEntity">数据项类型</typeparam>
public class SqlSugarPagedList<TEntity>
{
    /// <summary>
    /// 当前页码（从 1 开始）
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 每页记录数（页容量）
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 当前页的数据项列表
    /// </summary>
    public IEnumerable<TEntity> Items { get; set; } = Array.Empty<TEntity>();

    /// <summary>
    /// 是否存在上一页
    /// </summary>
    public bool HasPrevPage => PageNumber > 1;

    /// <summary>
    /// 是否存在下一页
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// SqlSugar 分页扩展方法
/// </summary>
public static class SqlSugarPagedExtensions
{
    /// <summary>
    /// 将 SqlSugar 查询投影并分页（同步）
    /// </summary>
    public static SqlSugarPagedList<TResult> ToPagedList<TEntity, TResult>(
        this ISugarQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, TResult>> selector)
    {
        ValidatePageParams(pageNumber, pageSize);
        _ = query ?? throw new ArgumentNullException(nameof(query));
        _ = selector ?? throw new ArgumentNullException(nameof(selector));

        var total = 0;
        var items = query.ToPageList(pageNumber, pageSize, ref total, selector);
        return CreateSqlSugarPagedList(items, total, pageNumber, pageSize);
    }

    /// <summary>
    /// 将 SqlSugar 查询直接分页（同步）
    /// </summary>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(
        this ISugarQueryable<TEntity> query,
        int pageNumber,
        int pageSize)
    {
        ValidatePageParams(pageNumber, pageSize);
        _ = query ?? throw new ArgumentNullException(nameof(query));

        var total = 0;
        var items = query.ToPageList(pageNumber, pageSize, ref total);
        return CreateSqlSugarPagedList(items, total, pageNumber, pageSize);
    }

    /// <summary>
    /// 将 SqlSugar 查询投影并分页（异步）
    /// </summary>
    public static async Task<SqlSugarPagedList<TResult>> ToPagedListAsync<TEntity, TResult>(
        this ISugarQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, TResult>> selector)
    {
        ValidatePageParams(pageNumber, pageSize);
        _ = query ?? throw new ArgumentNullException(nameof(query));
        _ = selector ?? throw new ArgumentNullException(nameof(selector));

        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageNumber, pageSize, total, selector);
        return CreateSqlSugarPagedList(items, total, pageNumber, pageSize);
    }

    /// <summary>
    /// 将 SqlSugar 查询直接分页（异步）
    /// </summary>
    public static async Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(
        this ISugarQueryable<TEntity> query,
        int pageNumber,
        int pageSize)
    {
        ValidatePageParams(pageNumber, pageSize);
        _ = query ?? throw new ArgumentNullException(nameof(query));

        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageNumber, pageSize, total);
        return CreateSqlSugarPagedList(items, total, pageNumber, pageSize);
    }

    /// <summary>
    /// 对内存集合进行分页（适用于小数据集，不推荐大数据量使用）
    /// </summary>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(
        this IEnumerable<TEntity> source,
        int pageNumber,
        int pageSize)
    {
        ValidatePageParams(pageNumber, pageSize);
        _ = source ?? throw new ArgumentNullException(nameof(source));

        var total = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return CreateSqlSugarPagedList(items, total, pageNumber, pageSize);
    }

    /// <summary>
    /// 创建 SqlSugarPagedList 分页结果对象
    /// </summary>
    private static SqlSugarPagedList<TEntity> CreateSqlSugarPagedList<TEntity>(
        IEnumerable<TEntity> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        // 防止除零，虽然 ValidatePageParams 已保证 pageSize >= 1
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new SqlSugarPagedList<TEntity>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items ?? Array.Empty<TEntity>(),
        };
    }

    /// <summary>
    /// 验证分页参数（页码和页大小必须 ≥1）
    /// </summary>
    private static void ValidatePageParams(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("页码必须大于等于 1。", nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentException("页容量必须大于等于 1。", nameof(pageSize));
    }
}