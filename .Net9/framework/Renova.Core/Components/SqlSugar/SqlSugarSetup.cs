using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Renova.Core.Apps;
using SqlSugar;
using Yitter.IdGenerator;

namespace Renova.Core.Components.SqlSugar;

public static class SqlSugarSetup
{
    /// <summary>
    /// 添加SqlSugar服务
    /// </summary>
    public static IServiceCollection AddSqlSugar(this IServiceCollection services)
    {
        //注册选项
        services.AddOptions<List<SqlSugarOptions>>()
                .BindConfiguration(SqlSugarOptions.SectionName)
                .ValidateDataAnnotations();

        //配置雪花Id生成器
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions { WorkerId = 1 });
        StaticConfig.CustomSnowFlakeFunc = YitIdHelper.NextId;

        //注册SqlSugar客户端
        services.AddSingleton<ISqlSugarClient>(s =>
        {
            var dbConfigs = s.GetRequiredService<IOptions<List<SqlSugarOptions>>>().Value;
            var sqlSugar = new SqlSugarScope(dbConfigs.Adapt<List<ConnectionConfig>>(), sqlSugarClient =>
            {
                dbConfigs.ForEach(config =>
                {
                    ISqlSugarClient db = sqlSugarClient.GetConnectionScope(config.ConfigId);
                    ConfigureDbAop(db, config.EnableConsoleSql);
                });
            });

            return sqlSugar;
        });

        services.AddScoped(typeof(SqlSugarRepository<>));  //注册仓储（注意：多库仓储的仓储对象不能单例注入）
        services.AddScoped<IUnitOfWork, SqlSugarUnitOfWork>(); //注册工作单元
        return services;
    }

    /// <summary>
    /// 配置SqlSugar客户端的AOP事件
    /// </summary>
    public static void ConfigureDbAop(ISqlSugarClient db, bool enableConsoleSql)
    {
        // 初始化AOP事件处理器
        Action<ISqlSugarClient>? onClientConfig = null;
        Action<string, SugarParameter[]>? onLogExecuting = null;
        Action<string, SugarParameter[]>? onLogExecuted = null;
        Action<object, DataFilterModel>? dataExecuting = null;
        Action<object, DataAfterModel>? dataExecuted = null;
        Action<SqlSugarException>? onError = null;

        var sqlSugarAopProviders = App.GetServices<ISqlSugarAopProvider>() ?? Enumerable.Empty<ISqlSugarAopProvider>(); ;

        // 按执行顺序聚合所有依赖项的AOP处理器
        //DataExecuting → OnLogExecuting → 执行 SQL → OnLogExecuted → DataExecuted

        foreach (var dependency in sqlSugarAopProviders.OrderBy(x => x.ExecutionOrder))
        {
            onClientConfig += dependency.OnSqlSugarClientConfig;
            dataExecuting += (data, entityInfo) => dependency.DataExecuting(db, data, entityInfo);
            onLogExecuting += (sql, parameters) => dependency.OnLogExecuting(db, sql, enableConsoleSql, parameters);
            onLogExecuted += (sql, parameters) => dependency.OnLogExecuted(db, sql, parameters);
            dataExecuted += (data, entityInfo) => dependency.DataExecuted(db, data, entityInfo);
            onError += (exception) => dependency.OnError(exception);
        }

        // 配置SqlSugar客户端
        onClientConfig?.Invoke(db);

        // 设置AOP事件
        db.Aop.OnLogExecuting = onLogExecuting;
        db.Aop.OnLogExecuted = onLogExecuted;
        db.Aop.DataExecuting = dataExecuting;
        db.Aop.DataExecuted = dataExecuted;
        db.Aop.OnError = onError;
    }
}
