using Microsoft.Extensions.DependencyInjection;
using NewLife.Caching;
using NewLife.Caching.Services;
using Renova.Core.Apps;

namespace Renova.Core.Components.Cache;

public static class CacheSetup
{
    /// <summary>
    /// 缓存注册
    /// </summary>
    public static void AddCache(this IServiceCollection services)
    {
        // 注册选项
        services.AddOptions<CacheOptions>()
            .BindConfiguration(CacheOptions.SectionName)
            .ValidateDataAnnotations();

        var cacheOptions = App.GetOptions<CacheOptions>();

        if (cacheOptions.CacheType == "Redis" && cacheOptions.Redis != null)
        {
            var redis = new FullRedis(cacheOptions.Redis);
            services.AddSingleton<ICacheProvider>(p => new RedisCacheProvider(p) { Cache = redis });
        }
        else
        {
            // 内存缓存兜底
            services.AddSingleton<ICacheProvider, CacheProvider>();
        }
    }
}