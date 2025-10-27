using SqlSugar;
using System.Reflection;

namespace Renova.Core.Components.SqlSugar;

public interface ISqlSugarAopProvider
{
    //执行顺序：
    //DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted

    // SQL 执行发生异常时触发:
    // OnError

    /// <summary>
    /// 获取执行顺序
    /// </summary>
    int ExecutionOrder { get; }

    /// <summary>
    /// SqlSugar客户端配置时触发
    /// </summary>
    /// <param name="sqlSugarClient">SqlSugar客户端实例</param>
    void OnSqlSugarClientConfig(ISqlSugarClient sqlSugarClient);

    /// <summary>
    /// 数据执行前触发
    /// </summary>
    /// <param name="oldValue">原始值</param>
    /// <param name="entityInfo">实体信息</param>
    void DataExecuting(ISqlSugarClient db, object oldValue, DataFilterModel entityInfo);

    /// <summary>
    /// SQL执行前触发
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="enableConsoleSql">启用控制台打印SQL</param>
    /// <param name="parameters">SQL参数</param>
    void OnLogExecuting(ISqlSugarClient db, string sql, bool enableConsoleSql, SugarParameter[] pars);

    /// <summary>
    /// SQL执行后触发
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    void OnLogExecuted(ISqlSugarClient db, string sql, SugarParameter[] pars);

    /// <summary>
    /// 数据执行后触发
    /// </summary>
    /// <param name="oldValue">原始值</param>
    /// <param name="entityInfo">实体信息</param>
    void DataExecuted(ISqlSugarClient db, object oldValue, DataAfterModel entityInfo);

    /// <summary>
    /// 实体服务配置
    /// </summary>
    /// <param name="propertyInfo">属性信息</param>
    /// <param name="entityColumnInfo">实体列信息</param>
    void EntityService(PropertyInfo propertyInfo, EntityColumnInfo entityColumnInfo);

    /// <summary>
    /// SQL执行发生异常时触发
    /// </summary>
    /// <param name="exception">SqlSugar异常信息</param>
    void OnError(SqlSugarException exception);
}
