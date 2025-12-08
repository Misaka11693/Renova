using Renova.Core.Apps;
using Serilog;
using SqlSugar;
using System.Reflection;
using Yitter.IdGenerator;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// SqlSugar AOP 提供器，用于统一处理数据库操作的拦截逻辑（如自动填充、日志、超时等）。
/// </summary>
/// <remarks>
/// 执行顺序：
/// <para>DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted</para>
/// </remarks>
public class SqlSugarAopProvider : ISqlSugarAopProvider, IScopedDependency
{
    /// <summary>
    /// 执行顺序，值越小优先级越高。
    /// </summary>
    public int ExecutionOrder => 0;

    /// <summary>
    /// SQL 执行超时时间（单位：秒）。
    /// </summary>
    private const int COMMAND_TIMEOUT_SECONDS = 30;

    /// <summary>
    /// 慢查询阈值（单位：秒）。超过此时间的 SQL 将被记录为警告日志。
    /// </summary>
    private const double SLOW_QUERY_THRESHOLD_SECONDS = 5.0;

    /// <summary>
    /// 在 SqlSugar 客户端初始化配置时触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    public void OnSqlSugarClientConfig(ISqlSugarClient db)
    {
        #region Ado 配置

        // 设置 SQL 命令超时
        db.Ado.CommandTimeOut = COMMAND_TIMEOUT_SECONDS;

        #endregion

        #region 全局过滤器

        // 全局软删除过滤：自动排除 IsDeleted = true 的记录
        db.QueryFilter.AddTableFilter<ISoftDeleteFilter>(it => it.IsDeleted == false);

        // 全局租户过滤：仅查询当前用户所属租户的数据 (待验证：每次执行是否都会触发此过滤器)
        var tenantIdClaim = App.User?.FindFirst(ClaimConst.TenantId)?.Value;
        if (!string.IsNullOrWhiteSpace(tenantIdClaim) && long.TryParse(tenantIdClaim, out var tenantId))
        {
            db.QueryFilter.AddTableFilter<ITenantIdFilter>(u => u.TenantId == tenantId);
        }

        #endregion
    }

    /// <summary>
    /// 在实体数据操作（插入/更新）前触发，自动填充审计字段。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="oldValue">原始值（未使用）</param>
    /// <param name="entityInfo">实体操作上下文信息</param>
    public void DataExecuting(ISqlSugarClient db, object oldValue, DataFilterModel entityInfo)
    {
        var currentUser = App.User ?? throw new InvalidOperationException("当前操作未登录，无法获取用户上下文信息。");

        #region 插入操作（自动填充主键、创建时间、用户、组织、租户等字段）

        if (entityInfo.OperationType == DataFilterType.InsertByObject)
        {
            // 1. 自动生成雪花 ID（仅适用于非自增、long 类型主键）
            if (entityInfo.EntityColumnInfo.IsPrimarykey
                && !entityInfo.EntityColumnInfo.IsIdentity
                && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
            {
                var currentId = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                if (currentId == null || (long)currentId == 0)
                {
                    entityInfo.SetValue(YitIdHelper.NextId());
                }
            }

            // 2.自动填充创建时间
            if (entityInfo.PropertyName == nameof(EntityBase.CreateTime))
            {
                var createTime = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                if (createTime == null || createTime.Equals(DateTime.MinValue))
                {
                    entityInfo.SetValue(DateTime.Now);
                }
            }

            // 3. 自动填充租户 ID（long 类型）
            if (entityInfo.PropertyName == nameof(EntityTenantId.TenantId))
            {
                var tenantId = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as long?;
                if (!tenantId.HasValue || tenantId == 0)
                {
                    var tenantIdStr = currentUser.FindFirst(ClaimConst.TenantId)?.Value;
                    if (long.TryParse(tenantIdStr, out var tid))
                    {
                        entityInfo.SetValue(tid);
                    }
                }
            }

            // 4. 自动填充创建用户 ID
            if (entityInfo.PropertyName == nameof(EntityBase.CreateUserId))
            {
                var userId = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as long?;
                if (!userId.HasValue || userId == 0)
                {
                    var userIdStr = currentUser.FindFirst(ClaimConst.UserId)?.Value;
                    if (long.TryParse(userIdStr, out var uid))
                    {
                        entityInfo.SetValue(uid);
                    }
                }
            }

            // 5. 自动填充创建用户名
            if (entityInfo.PropertyName == nameof(EntityBase.CreateUserName))
            {
                var userName = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as string;
                if (string.IsNullOrEmpty(userName))
                {
                    entityInfo.SetValue(currentUser.FindFirst(ClaimConst.RealName)?.Value);
                }
            }

            // 6. 自动填充创建组织 ID
            if (entityInfo.PropertyName == nameof(EntityBaseData.CreateOrgId))
            {
                var orgId = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as long?;
                if (!orgId.HasValue || orgId == 0)
                {
                    var orgIdStr = currentUser.FindFirst(ClaimConst.OrgId)?.Value;
                    if (long.TryParse(orgIdStr, out var oid))
                    {
                        entityInfo.SetValue(oid);
                    }
                }
            }

            // 7. 创建组织名称
            if (entityInfo.PropertyName == nameof(EntityBaseData.CreateOrgName))
            {
                var orgName = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as string;
                if (string.IsNullOrEmpty(orgName))
                {
                    entityInfo.SetValue(currentUser.FindFirst(ClaimConst.OrgName)?.Value);
                }
            }
        }
        #endregion

        #region 更新操作（自动填充更新时间、更新人信息）

        if (entityInfo.OperationType == DataFilterType.UpdateByObject)
        {
            // 1. 自动填充更新时间
            if (entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
            {
                entityInfo.SetValue(DateTime.Now);
            }

            // 2. 自动填充更新用户 ID
            if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserId))
            {
                var userIdStr = currentUser.FindFirst(ClaimConst.UserId)?.Value;
                if (long.TryParse(userIdStr, out var uid))
                {
                    entityInfo.SetValue(uid);
                }
            }

            // 3. 自动填充更新用户名
            if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserName))
            {
                entityInfo.SetValue(currentUser.FindFirst(ClaimConst.RealName)?.Value);
            }
        }

        #endregion
    }

    /// <summary>
    /// 在 SQL 执行前触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="sql">原始 SQL 语句</param>
    /// <param name="enableConsoleSql">是否启用控制台 SQL 输出</param>
    /// <param name="pars">SQL 参数数组</param>
    public void OnLogExecuting(ISqlSugarClient db, string sql, bool enableConsoleSql, SugarParameter[] pars)
    {
        #region 控制台输出 SQL 语句 （仅当 enableConsoleSql = true 时生效）

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

        #endregion
    }

    /// <summary>
    /// 在 SQL 执行后触发。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="sql">SQL 语句</param>
    /// <param name="pars">SQL 参数数组</param>
    public void OnLogExecuted(ISqlSugarClient db, string sql, SugarParameter[] pars)
    {
        #region 记录超过慢查询阈值的 SQL

        if (db.Ado.SqlExecutionTime.TotalSeconds <= SLOW_QUERY_THRESHOLD_SECONDS)
            return;

        var trace = db.Ado.SqlStackTrace;
        var logMessage = $@"【慢查询】{db.CurrentConnectionConfig.ConfigId} | 耗时: {db.Ado.SqlExecutionTime.TotalSeconds:F2}秒
【文件】{trace.FirstFileName}:{trace.FirstLine}
【方法】{trace.FirstMethodName}
【SQL】{UtilMethods.GetNativeSql(sql, pars)}";

        Log.Warning(logMessage);

        #endregion
    }

    /// <summary>
    /// 在实体数据操作完成后触发（当前未使用）。
    /// </summary>
    /// <param name="db">当前 SqlSugar 客户端实例</param>
    /// <param name="oldValue">原始值</param>
    /// <param name="entityInfo">实体操作后上下文</param>
    public void DataExecuted(ISqlSugarClient db, object oldValue, DataAfterModel entityInfo)
    {
        // TODO: 
    }

    /// <summary>
    /// 实体列配置时触发（当前未使用）。
    /// </summary>
    /// <param name="propertyInfo">属性信息</param>
    /// <param name="entityColumnInfo">实体列信息</param>
    public void EntityService(PropertyInfo propertyInfo, EntityColumnInfo entityColumnInfo)
    {
        // TODO: 
    }

    /// <summary>
    /// SQL 执行发生异常时触。
    /// </summary>
    /// <param name="exception">SqlSugar 异常对象</param>
    public void OnError(SqlSugarException exception)
    {
        #region 记录异常 SQL 详情

        if (exception.Parametres is not SugarParameter[] parameters)
            return;

        //        var logMessage = $@"【SQL执行异常】{DateTime.Now:yyyy-MM-dd HH:mm:ss}
        //【数据库】{exception.SqlSugarClient?.CurrentConnectionConfig.ConfigId}
        //【SQL】{UtilMethods.GetNativeSql(exception.Sql, parameters)}";

        var logMessage = $@"【SQL执行异常】{DateTime.Now:yyyy-MM-dd HH:mm:ss}
【SQL语句】：{UtilMethods.GetNativeSql(exception.Sql, parameters)}";

        Log.Error(exception, logMessage);

        #endregion
    }
}
