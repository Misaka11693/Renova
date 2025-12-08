using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;
using Renova.Core.Components.Cache;

namespace Renova.Core.Components.Job;

/// <summary>
/// Job服务配置扩展
/// </summary>
public static class JobSetup
{
    /// <summary>
    /// 定时任务服务配置
    /// </summary>
    public static IServiceCollection AddHangfireJob(this IServiceCollection services)
    {
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage());
    
        services.AddHangfireServer(options =>
        {
            options.ServerName = Environment.MachineName;
            //options.Queues = new[] { "critical", "default" }; 
        });

        return services;
    }

    /// <summary>
    /// 存储方式配置
    /// </summary>
    public static IGlobalConfiguration UseStorage(this IGlobalConfiguration configuration)
    {
        var cacheOptions = App.GetOptions<CacheOptions>();

        if (cacheOptions.CacheType == "Redis" && cacheOptions.Redis != null)
        {
            var redis = cacheOptions.Redis;
            var redisString = redis.ToStackExchangeConnectionString();
            if (string.IsNullOrEmpty(redisString))
            {
                throw new InvalidOperationException("Redis 配置不完整：Server 未设置且 Configuration 为空，无法生成连接字符串。请检查 CacheOptions:Redis 配置。");
            }

            // 使用独立 DB,如果未设置则使用默认 DB
            var hangfireDb = redis.JobDb ?? redis.Db;

            return configuration.UseRedisStorage(redisString,
                new RedisStorageOptions()
                {
                    Db = hangfireDb,
                    Prefix = "Renova:HangfireJob",
                    InvisibilityTimeout = TimeSpan.FromHours(1)
                }).WithJobExpirationTimeout(TimeSpan.FromDays(7));
        }
        return configuration.UseMemoryStorage();
    }
}
