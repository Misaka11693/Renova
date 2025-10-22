using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core.Components.SqlSugar;

public interface ISqlSugarAopInterceptor
{
    //执行顺序：
    //DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted

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
    /// <param name="parameters">SQL参数</param>
    void OnLogExecuting(ISqlSugarClient db, string sql, SugarParameter[] pars);

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
}
