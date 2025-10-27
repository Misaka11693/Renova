using SqlSugar;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// SqlSugar仓储接口
/// </summary>
public interface ISqlSugarRepository<T> : ISugarRepository, ISimpleClient<T> where T : class, new()
{
}
