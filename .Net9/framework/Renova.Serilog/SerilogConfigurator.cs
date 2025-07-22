using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Renova.Serilog;

public static class SerilogConfigurator
{
    /// <summary>
    /// 初始化全局日志配置
    /// </summary>
    /// <returns>配置完成的 Logger 实例</returns>
    public static Logger Init()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Logger(ConfigureAllLog)       // 所有级别日志
            .WriteTo.Logger(ConfigureVerboseLog)   // 详细级别
            .WriteTo.Logger(ConfigureDebugLog)     // 调试级别
            .WriteTo.Logger(ConfigureInfoLog)      // 信息级别
            .WriteTo.Logger(ConfigureWarnLog)      // 警告级别
            .WriteTo.Logger(ConfigureErrorLog)     // 错误级别
            .WriteTo.Logger(ConfigureFatalLog)     // 致命级别
            .WriteTo.Console()
            .CreateLogger();
    }

    #region 私有配置方法

    /// <summary>
    /// 配置所有级别日志
    /// </summary>
    private static void ConfigureAllLog(LoggerConfiguration config)
    {
        config
            .WriteTo.Async(a => a.File(
            path: "logs/all/all-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ));
    }

    /// <summary>
    /// 配置详细级别日志 (最低级别)
    /// </summary>
    private static void ConfigureVerboseLog(LoggerConfiguration config)
    {
        config
           .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Verbose)
           .WriteTo.Async(a => a.File(
               path: "logs/verbose/verbose-.log",
               rollingInterval: RollingInterval.Day
           ));
    }

    /// <summary>
    /// 配置调试级别日志
    /// </summary>
    private static void ConfigureDebugLog(LoggerConfiguration config)
    {
        config
           .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
           .WriteTo.Async(a => a.File(
               path: "logs/debug/debug-.log",
               rollingInterval: RollingInterval.Day
           ));
    }

    /// <summary>
    /// 配置信息级别日志
    /// </summary>
    private static void ConfigureInfoLog(LoggerConfiguration config)
    {
        config
           .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
           .WriteTo.Async(a => a.File(
               path: "logs/info/info-.log",
               rollingInterval: RollingInterval.Day
           ));
    }

    /// <summary>
    /// 配置警告级别日志
    /// </summary>
    private static void ConfigureWarnLog(LoggerConfiguration config)
    {
        config
           .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
           .WriteTo.Async(a => a.File(
               path: "logs/warn/warn-.log",
               rollingInterval: RollingInterval.Day
           ));
    }

    /// <summary>
    /// 配置错误级别日志
    /// </summary>
    private static void ConfigureErrorLog(LoggerConfiguration config)
    {
        config
           .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
           .WriteTo.Async(a => a.File(
               path: "logs/error/error-.log",
               rollingInterval: RollingInterval.Day
           ));
    }

    /// <summary>
    /// 配置致命级别日志 (最高级别)
    /// </summary>
    private static void ConfigureFatalLog(LoggerConfiguration config)
    {
        config
           .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
           .WriteTo.Async(a => a.File(
               path: "logs/fatal/fatal-.log",
               rollingInterval: RollingInterval.Day
           ));
    }

    #endregion
}
