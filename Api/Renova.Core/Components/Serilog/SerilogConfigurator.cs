using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Renova.Core.Components.Serilog.Options;
using Renova.Core.Components.Serilog.Sinks.Elasticsearch;
using Renova.Core.Components.Serilog.Sinks.File;
using Serilog;
using Serilog.Core;
using Serilog.Events;
namespace Renova.Core.Components.Serilog;

/// <summary>
/// Serilog 配置
/// </summary>
public static class SerilogConfigurator
{
    /// <summary>
    /// 初始化 Serilog 启动日志
    /// </summary>
    public static Logger CreateBootstrapLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Logger(ConfigureBootstrapFileLog)
#if DEBUG
            .WriteTo.Console()
#endif
            .CreateLogger();
    }

    /// <summary>
    /// 配置 HostBuilder 使用 Serilog
    /// </summary>
    public static IHostBuilder UseConfiguredSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, logger) =>
        {
            // 会在 var app = builder.Build() 时触发此配置

            var options = context.Configuration.GetSection("SerilogOptions").Get<SerilogOptions>() ?? new SerilogOptions();

            logger
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            // 配置文件日志
            FileLogConfigurator.Configure(logger, options, services);

            // 配置 Elasticsearch 日志
            ElasticsearchLogConfigurator.Configure(logger, options, services);
#if DEBUG
            logger.WriteTo.Console();
#endif

        });
    }

    #region 配置日志

    /// <summary>
    /// 配置启动时日志
    /// </summary>
    private static void ConfigureBootstrapFileLog(LoggerConfiguration config)
    {
        config
            .WriteTo.Async(a => a.File(
            path: "logs/bootstrap/bootstrap-.log",
            rollingInterval: RollingInterval.Day
            //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ));
    }

    #endregion
}
