using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Components.Serilog.Options;
using Renova.Core.Components.Serilog.Sinks.Elasticsearch.Ecs;
using Renova.Core.Components.Serilog.Sinks.Elasticsearch.Policies;
using Serilog;

namespace Renova.Core.Components.Serilog.Sinks.Elasticsearch;

/// <summary>
/// Elasticsearch 日志配置器
/// </summary>
public static class ElasticsearchLogConfigurator
{
    /// <summary>
    /// 配置 Elasticsearch 日志
    /// </summary>
    public static void Configure(LoggerConfiguration logger, SerilogOptions options, IServiceProvider services)
    {
        if (options?.EnableElk != true)
            return;

        if (string.IsNullOrWhiteSpace(options.ElasticsearchUrl))
        {
            throw new InvalidOperationException("EnableElk=true 时必须配置 ElasticsearchUrl");
        }

        var policy = services.GetRequiredService<ElasticsearchLogWritePolicy>();

        var httpAccessor = services.GetService<HttpContextAccessor>();

        // 配置 Elasticsearch Sink
        logger.WriteTo.Logger(lc =>
        {
            lc.Filter.ByIncludingOnly(policy.ShouldWrite);

            lc.WriteTo.Elasticsearch<MyEcsDocument>(
                new[] { new Uri(options.ElasticsearchUrl!) },

                // 数据流和格式化配置
                opts =>
                {
                    opts.DataStream = new DataStreamName("Apilogs", "console-example", "demo");
                    opts.TextFormatting = new EcsFormatterConfig(httpAccessor);
                },

                // 认证配置
                transport =>
                {
                    transport.Authentication(
                        new BasicAuthentication("kokkoro", "123456"));
                });
        });
    }
}
