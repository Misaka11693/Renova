using SqlSugar;
using System.Linq.Expressions;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// SqlSugar 仓储接口
/// </summary>
public interface ISqlSugarRepository<T> : ISugarRepository, ISimpleClient<T> where T : class, new()
{
    /// <summary>
    /// 逻辑删除指定条件的数据。
    /// </summary>
    Task<int> SoftDeleteAsync(Expression<Func<T, bool>> expression);
}
