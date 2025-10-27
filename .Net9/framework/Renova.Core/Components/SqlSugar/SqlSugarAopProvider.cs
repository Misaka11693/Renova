using Renova.Core.Apps;
using Renova.Core.Components.DependencyInjection.Dependencies;
using Serilog;
using SqlSugar;
using System.Reflection;
using Yitter.IdGenerator;

namespace Renova.Core.Components.SqlSugar;

public class SqlSugarAopProvider : ISqlSugarAopProvider, ITransientDependency
{
    // 执行顺序：
    // DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted

    private const int COMMAND_TIMEOUT_SECONDS = 30; // SQL执行超时时间，单位秒
    private const double SLOW_QUERY_THRESHOLD_SECONDS = 5.0; // 慢查询阈值，单位秒

    public int ExecutionOrder => 0;

    /// <summary>
    /// SqlSugar客户端配置时触发
    /// </summary>
    /// <param name="sqlSugarClient">SqlSugar客户端实例</param>
    public void OnSqlSugarClientConfig(ISqlSugarClient db)
    {
        // 设置SQL执行超时时间
        db.Ado.CommandTimeOut = COMMAND_TIMEOUT_SECONDS;

        // 软删除全局过滤
        db.QueryFilter.AddTableFilter<ISoftDeleteFilter>(it => it.IsDeleted == false);

        // 租户过滤 ( todo : 验证是否每次执行都会触发此过滤器)
        var tenantIdClaim = App.User?.FindFirst(ClaimConst.TenantId)?.Value;
        if (!string.IsNullOrWhiteSpace(tenantIdClaim) && long.TryParse(tenantIdClaim, out var tenantId))
        {
            db.QueryFilter.AddTableFilter<ITenantIdFilter>(u => u.TenantId == tenantId);
        }
    }

    /// <summary>
    /// 数据执行前触发
    /// </summary>
    /// <param name="oldValue">原始值</param>
    /// <param name="entityInfo">实体信息</param>
    public void DataExecuting(ISqlSugarClient db, object oldValue, DataFilterModel entityInfo)
    {
        // 插入操作
        if (entityInfo.OperationType == DataFilterType.InsertByObject)
        {
            // 1.主键Id
            if (entityInfo.EntityColumnInfo.IsPrimarykey && !entityInfo.EntityColumnInfo.IsIdentity && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
            {
                var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                if (id == null || (long)id == 0)
                    entityInfo.SetValue(YitIdHelper.NextId());
            }

            // 2.创建时间
            else if (entityInfo.PropertyName == nameof(EntityBase.CreateTime))
            {
                var createTime = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue)!;
                if (createTime == null || createTime.Equals(DateTime.MinValue))
                    entityInfo.SetValue(DateTime.Now);
            }

            if (App.User == null)
            {
                throw new Exception("当前操作未登录，无法获取用户信息！");
            }

            dynamic entityValue = entityInfo.EntityValue;
            if (entityInfo.PropertyName == nameof(EntityTenantId.TenantId))
            {
                var tenantId = entityValue.TenantId;
                if (tenantId == null || tenantId == 0)
                    entityInfo.SetValue(App.User.FindFirst(ClaimConst.TenantId)?.Value);
            }
            else if (entityInfo.PropertyName == nameof(EntityBase.CreateUserId))
            {
                var createUserId = entityValue.CreateUserId;
                if (createUserId == 0 || createUserId == null)
                    entityInfo.SetValue(App.User.FindFirst(ClaimConst.UserId)?.Value);
            }
            else if (entityInfo.PropertyName == nameof(EntityBase.CreateUserName))
            {
                var createUserName = entityValue.CreateUserName;
                if (string.IsNullOrEmpty(createUserName))
                    entityInfo.SetValue(App.User.FindFirst(ClaimConst.RealName)?.Value);
            }
            else if (entityInfo.PropertyName == nameof(EntityBaseData.CreateOrgId))
            {
                var createOrgId = entityValue.CreateOrgId;
                if (createOrgId == 0 || createOrgId == null)
                    entityInfo.SetValue(App.User.FindFirst(ClaimConst.OrgId)?.Value);
            }
            else if (entityInfo.PropertyName == nameof(EntityBaseData.CreateOrgName))
            {
                var createOrgName = entityValue.CreateOrgName;
                if (string.IsNullOrEmpty(createOrgName))
                    entityInfo.SetValue(App.User.FindFirst(ClaimConst.OrgName)?.Value);
            }
        }

        // 更新操作
        else if (entityInfo.OperationType == DataFilterType.UpdateByObject)
        {
            if (entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
                entityInfo.SetValue(DateTime.Now);
            else if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserId))
                entityInfo.SetValue(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            else if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserName))
                entityInfo.SetValue(App.User?.FindFirst(ClaimConst.RealName)?.Value);
        }
    }

    /// <summary>
    /// SQL执行前触发
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    public void OnLogExecuting(ISqlSugarClient db, string sql, bool enableConsoleSql, SugarParameter[] pars)
    {
        if (!enableConsoleSql) return;

        var config = db.CurrentConnectionConfig;
        var sqlUpper = sql.TrimStart().ToUpperInvariant();
        string operation;
        ConsoleColor color;

        if (sqlUpper.StartsWith("SELECT"))
        {
            operation = $"查询{config.ConfigId}库操作";
            color = ConsoleColor.Green;
        }
        else if (sqlUpper.StartsWith("INSERT") || sqlUpper.StartsWith("UPDATE"))
        {
            operation = $"修改{config.ConfigId}库操作";
            color = ConsoleColor.Blue;
        }
        else if (sqlUpper.StartsWith("DELETE"))
        {
            operation = $"删除{config.ConfigId}库操作";
            color = ConsoleColor.Red;
        }
        else
        {
            operation = $"{config.ConfigId}库操作";
            color = ConsoleColor.Gray;
        }

        // 设置颜色
        Console.ForegroundColor = color;
        Console.WriteLine($"=============={operation}==============");
        Console.WriteLine(UtilMethods.GetSqlString(config.DbType, sql, pars));
        Console.WriteLine($"=============={config.ConfigId}库操作结束==============");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// SQL执行后触发
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">SQL参数</param>
    public void OnLogExecuted(ISqlSugarClient db, string sql, SugarParameter[] pars)
    {
        // 执行时间超过5秒时
        if (db.Ado.SqlExecutionTime.TotalSeconds <= SLOW_QUERY_THRESHOLD_SECONDS) return;

        var trace = db.Ado.SqlStackTrace;
        var logMessage = $@"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}——超时SQL】
【所在文件名】：{trace.FirstFileName}
【代码行数】：{trace.FirstLine}
【方法名】：{trace.FirstMethodName}
【SQL语句】：{UtilMethods.GetNativeSql(sql, pars)}";

        Log.Warning(logMessage);
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

    /// <summary>
    /// SQL 执行发生异常时触发
    /// </summary>
    /// <param name="exception"></param>
    public void OnError(SqlSugarException exception)
    {
        if (exception.Parametres is not SugarParameter[] parameters) return;

        var logMessage = $@"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}——错误SQL】
【SQL语句】：{UtilMethods.GetNativeSql(exception.Sql, parameters)}";

        Log.Error(exception, logMessage);
    }
}
