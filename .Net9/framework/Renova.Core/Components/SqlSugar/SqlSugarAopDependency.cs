using Renova.Core.Components.DependencyInjection.Dependencies;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core.Components.SqlSugar;

public class SqlSugarAopDependency : ISqlSugarAopInterceptor, ITransientDependency
{
    //执行顺序：
    //DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted

    public int ExecutionOrder => 0;

    /// <summary>
    /// SqlSugar客户端配置时触发
    /// </summary>
    /// <param name="sqlSugarClient">SqlSugar客户端实例</param>
    public void OnSqlSugarClientConfig(ISqlSugarClient db)
    {
        //全局过滤软删除字段
        //db.QueryFilter.AddTableFilter<ISoftDelete>(it => it.IsDeleted == false);
    }

    /// <summary>
    /// 数据执行前触发
    /// </summary>
    /// <param name="oldValue">原始值</param>
    /// <param name="entityInfo">实体信息</param>
    public void DataExecuting(ISqlSugarClient db, object oldValue, DataFilterModel entityInfo)
    {
        if (entityInfo.OperationType == DataFilterType.InsertByObject)
        {
            if (entityInfo.PropertyName == "CreateTime")
            {
                entityInfo.SetValue(DateTime.Now);//修改CreateTime字段
            }
        }

        if (entityInfo.OperationType == DataFilterType.UpdateByObject)
        {
            if (entityInfo.PropertyName == "UpdateTime")
            {
                entityInfo.SetValue(DateTime.Now);//修改UpdateTime段
            }
        }
    }

    /// <summary>
    /// SQL执行前触发
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    public void OnLogExecuting(ISqlSugarClient db, string sql, SugarParameter[] pars)
    {
        var config = db.CurrentConnectionConfig;

        if (sql.StartsWith("SELECT"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteSqlLog($"查询{config.ConfigId}库操作");
        }
        if (sql.StartsWith("UPDATE") || sql.StartsWith("INSERT"))
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteSqlLog($"修改{config.ConfigId}库操作");
        }
        if (sql.StartsWith("DELETE"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteSqlLog($"删除{config.ConfigId}库操作");
        }
        Console.WriteLine(UtilMethods.GetSqlString(config.DbType, sql, pars));
        WriteSqlLog($"{config.ConfigId}库操作结束");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    /// <summary>
    /// SQL执行后触发
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    public void OnLogExecuted(ISqlSugarClient db, string sql, SugarParameter[] pars)
    {

    }

    /// <summary>
    /// 数据执行后触发
    /// </summary>
    /// <param name="oldValue">原始值</param>
    /// <param name="entityInfo">实体信息</param>
    public void DataExecuted(ISqlSugarClient db, object oldValue, DataAfterModel entityInfo)
    {

    }

    /// <summary>
    /// 实体服务配置
    /// </summary>
    /// <param name="propertyInfo">属性信息</param>
    /// <param name="entityColumnInfo">实体列信息</param>
    public void EntityService(PropertyInfo propertyInfo, EntityColumnInfo entityColumnInfo)
    {

    }

    private static void WriteSqlLog(string msg)
    {
        Console.WriteLine($"=============={msg}==============");
    }
}
