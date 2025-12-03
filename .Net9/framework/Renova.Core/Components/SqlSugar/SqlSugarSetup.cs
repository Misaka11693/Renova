using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;
using Renova.Core.Components.Const;
using Serilog;
using SqlSugar;
using System.Reflection;
using Yitter.IdGenerator;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
///  SqlSugar服务注册扩展
/// </summary>
public static class SqlSugarSetup
{
    //IsDefined - 判断类型/成员上是否有某个特性（Attribute）
    //IsAssignableTo - 判断类型是否可以赋值给另一个类型（包括接口实现）

    /// <summary>
    /// SqlSugar服务注册
    /// </summary>
    public static IServiceCollection AddSqlSugar(this IServiceCollection services)
    {
        // 注册选项
        services.AddOptions<DbConnectionOptions>()
                .BindConfiguration(DbConnectionOptions.SectionName)
                .ValidateDataAnnotations();

        // 配置雪花ID生成器
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions { WorkerId = 1 });
        StaticConfig.CustomSnowFlakeFunc = YitIdHelper.NextId;

        var dbOptions = App.GetConfig<DbConnectionOptions>(DbConnectionOptions.SectionName);

        SqlSugarScope sqlSugar = new(dbOptions.ConnectionConfigs.Adapt<List<ConnectionConfig>>(), sqlSugarClient =>
        {
            dbOptions.ConnectionConfigs.ForEach(config =>
            {
                ISqlSugarClient db = sqlSugarClient.GetConnectionScope(config.ConfigId);
                ConfigureDbAop(db, dbOptions.EnableConsoleSql);
            });
        });

        services.AddSingleton<ISqlSugarClient>(sqlSugar); // 注册 SqlSugar 客户端
        services.AddScoped(typeof(SqlSugarRepository<>));  // 注册仓储（注意：多库仓储的仓储对象不能单例注入）
        services.AddScoped<IUnitOfWork, SqlSugarUnitOfWork>(); //注册工作单元

        // 初始化数据库表结构及种子数据
        dbOptions.ConnectionConfigs.ForEach(config =>
        {
            InitDatabase(sqlSugar, config);
        });

        return services;
    }

    /// <summary>
    /// 配置 SqlSugar 客户端的 AOP 事件
    /// </summary>
    public static void ConfigureDbAop(ISqlSugarClient db, bool enableConsoleSql)
    {
        // 初始化 AOP 事件处理器
        Action<ISqlSugarClient>? onClientConfig = null;
        Action<string, SugarParameter[]>? onLogExecuting = null;
        Action<string, SugarParameter[]>? onLogExecuted = null;
        Action<object, DataFilterModel>? dataExecuting = null;
        Action<object, DataAfterModel>? dataExecuted = null;
        Action<SqlSugarException>? onError = null;

        var sqlSugarAopProviders = App.GetServices<ISqlSugarAopProvider>() ?? Enumerable.Empty<ISqlSugarAopProvider>(); ;

        // 按执行顺序聚合所有依赖项的 AOP 处理器
        // DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted

        foreach (var dependency in sqlSugarAopProviders.OrderBy(x => x.ExecutionOrder))
        {
            onClientConfig += dependency.OnSqlSugarClientConfig;
            dataExecuting += (data, entityInfo) => dependency.DataExecuting(db, data, entityInfo);
            onLogExecuting += (sql, parameters) => dependency.OnLogExecuting(db, sql, enableConsoleSql, parameters);
            onLogExecuted += (sql, parameters) => dependency.OnLogExecuted(db, sql, parameters);
            dataExecuted += (data, entityInfo) => dependency.DataExecuted(db, data, entityInfo);
            onError += (exception) => dependency.OnError(exception);
        }

        // 配置 SqlSugar 客户端
        onClientConfig?.Invoke(db);

        // 设置 AOP 事件
        db.Aop.OnLogExecuting = onLogExecuting;
        db.Aop.OnLogExecuted = onLogExecuted;
        db.Aop.DataExecuting = dataExecuting;
        db.Aop.DataExecuted = dataExecuted;
        db.Aop.OnError = onError;
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="db">SqlSugarScope 实例</param>
    /// <param name="config">数据库连接配置</param>
    private static void InitDatabase(SqlSugarScope db, DbConnectionConfig config)
    {
        var dbProvider = db.GetConnectionScope(config.ConfigId);

        // 初始化数据库
        if (config.DbSettings.EnableInitDb)
        {
            Log.Information($"初始化数据库 {config.DbType} - {config.ConfigId} - {config.ConnectionString}");
            if (config.DbType != DbType.Oracle) dbProvider.DbMaintenance.CreateDatabase();
        }

        // 初始化表结构
        if (config.TableSettings.EnableInitTable)
        {
            Log.Information($"初始化表结构 {config.DbType} - {config.ConfigId}");
            var entityTypes = GetEntityTypesForInit(config);
            InitializeTables(dbProvider, entityTypes, config);
        }

        // 初始化种子数据
        //if (config.SeedSettings.EnableInitSeed) InitSeedData(db, config);
    }

    /// <summary>
    /// 获取需要初始化的实体类型
    /// </summary>
    /// <param name="config">数据库连接配置</param>
    /// <returns>实体类型列表</returns>
    private static List<Type> GetEntityTypesForInit(DbConnectionConfig config)
    {
        return App.EffectiveTypes
            .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false))
            .Where(u => !u.GetCustomAttributes<IgnoreTableAttribute>().Any())
            .WhereIF(config.TableSettings.EnableIncreTable, u => u.IsDefined(typeof(IncreTableAttribute), false))
            .Where(u => IsEntityForConfig(u, config))
            .ToList();
    }

    /// <summary>
    /// 判断实体是否属于当前配置
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="config">数据库连接配置</param>
    /// <returns>是否属于当前配置</returns>
    private static bool IsEntityForConfig(Type entityType, DbConnectionConfig config)
    {
        switch (config.ConfigId.ToString())
        {
            case SqlSugarConst.MainConfigId:
                return entityType.GetCustomAttributes<SysTableAttribute>().Any() ||
                       (!entityType.GetCustomAttributes<LogTableAttribute>().Any() &&
                        !entityType.GetCustomAttributes<TenantAttribute>().Any());
            case SqlSugarConst.LogConfigId:
                return entityType.GetCustomAttributes<LogTableAttribute>().Any();
            default:
                {
                    var tenantAttribute = entityType.GetCustomAttribute<TenantAttribute>();
                    return tenantAttribute != null && tenantAttribute.configId.ToString() == config.ConfigId.ToString();
                }
        }
    }

    /// <summary>
    /// 初始化表结构
    /// </summary>
    /// <param name="dbProvider">SqlSugarScopeProvider 实例</param>
    /// <param name="entityTypes">实体类型列表</param>
    /// <param name="config">数据库连接配置</param>
    private static void InitializeTables(SqlSugarScopeProvider dbProvider, List<Type> entityTypes, DbConnectionConfig config)
    {
        int count = 0, sum = entityTypes.Count;
        var tasks = entityTypes.Select(entityType => Task.Run(() =>
        {
            Console.WriteLine($"初始化表结构 {entityType.FullName,-64} ({config.ConfigId} - {Interlocked.Increment(ref count):D003}/{sum:D003})");
            UpdateNullableColumns(dbProvider, entityType);
            InitializeTable(dbProvider, entityType);
        }));

        Task.WhenAll(tasks).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 更新表中不存在于实体的字段为可空
    /// </summary>
    /// <param name="dbProvider">SqlSugarScopeProvider 实例</param>
    /// <param name="entityType">实体类型</param>
    private static void UpdateNullableColumns(SqlSugarScopeProvider dbProvider, Type entityType)
    {
        var entityInfo = dbProvider.EntityMaintenance.GetEntityInfo(entityType);
        var dbColumns = dbProvider.DbMaintenance.GetColumnInfosByTableName(entityInfo.DbTableName) ?? new List<DbColumnInfo>();

        foreach (var dbColumn in dbColumns.Where(c => !c.IsPrimarykey && entityInfo.Columns.All(u => u.DbColumnName != c.DbColumnName)))
        {
            dbColumn.IsNullable = true;
            dbProvider.DbMaintenance.UpdateColumn(entityInfo.DbTableName, dbColumn);
        }
    }

    /// <summary>
    /// 初始化表
    /// </summary>
    /// <param name="dbProvider">SqlSugarScopeProvider 实例</param>
    /// <param name="entityType">实体类型</param>
    private static void InitializeTable(SqlSugarScopeProvider dbProvider, Type entityType)
    {
        if (entityType.GetCustomAttribute<SplitTableAttribute>() == null)
        {
            dbProvider.CodeFirst.InitTables(entityType);
        }
        else
        {
            dbProvider.CodeFirst.SplitTables().InitTables(entityType);
        }
    }
}
