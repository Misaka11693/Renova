using SqlSugar;
using System.Reflection;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// 定义 SqlSugar AOP（面向切面编程）拦截器的契约，用于在数据操作的不同阶段注入自定义逻辑。
/// </summary>
/// <remarks>
/// 方法执行顺序如下：
/// <para>DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted</para>
/// <para>若 SQL 执行过程中发生异常，则触发 <see cref="OnError"/>。</para>
/// </remarks>
public interface ISqlSugarAopProvider
{
    /// <summary>
    /// 当前 AOP 提供器的执行顺序。值越小，优先级越高。
    /// </summary>
    int ExecutionOrder { get; }

    /// <summary>
    /// 在 SqlSugar 客户端初始化配置完成后触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    void OnSqlSugarClientConfig(ISqlSugarClient db);

    /// <summary>
    /// 在实体数据操作（插入/更新）前触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="oldValue">实体原始值</param>
    /// <param name="entityInfo">实体操作上下文信息</param>
    void DataExecuting(ISqlSugarClient db, object oldValue, DataFilterModel entityInfo);

    /// <summary>
    /// 在 SQL 语句执行前触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="sql">即将执行的原始 SQL 语句</param>
    /// <param name="enableConsoleSql">是否启用控制台 SQL 输出（由配置决定）</param>
    /// <param name="pars">SQL 参数数组</param>
    void OnLogExecuting(ISqlSugarClient db, string sql, bool enableConsoleSql, SugarParameter[] pars);

    /// <summary>
    /// 在 SQL 语句执行完成后触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="sql">已执行的 SQL 语句</param>
    /// <param name="pars">SQL 参数数组</param>
    void OnLogExecuted(ISqlSugarClient db, string sql, SugarParameter[] pars);

    /// <summary>
    /// 在实体数据操作完成后触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="oldValue">实体原始值</param>
    /// <param name="entityInfo">实体操作后的上下文信息</param>
    void DataExecuted(ISqlSugarClient db, object oldValue, DataAfterModel entityInfo);

    /// <summary>
    /// 在实体列元数据构建时触发。
    /// </summary>
    /// <param name="propertyInfo">实体类的属性信息</param>
    /// <param name="entityColumnInfo">对应的数据库列元数据</param>
    void EntityService(PropertyInfo propertyInfo, EntityColumnInfo entityColumnInfo);

    /// <summary>
    /// 在 SQL 执行过程中发生异常时触发。
    /// </summary>
    /// <param name="exception">SqlSugar 抛出的异常对象</param>
    void OnError(SqlSugarException exception);
}