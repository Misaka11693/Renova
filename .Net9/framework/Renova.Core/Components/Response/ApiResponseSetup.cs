using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Renova.Core.Components.Response;

/// <summary>
/// 统一 API 响应配置扩展
/// </summary>
public static class ApiResponseSetup
{
    /// <summary>
    /// 添加默认的统一 API 响应过滤器（使用 <see cref="ApiResponseFilter"/>）
    /// </summary>
    public static IServiceCollection AddApiResponseFilter(
        this IServiceCollection services,
        Action<MvcOptions>? configure = default)
    {
        return services.AddApiResponseFilter<ApiResponseFilter>(configure);
    }

    /// <summary>
    /// 添加指定类型的统一 API 响应过滤器
    /// </summary>
    public static IServiceCollection AddApiResponseFilter<TFilter>(
        this IServiceCollection services,
        Action<MvcOptions>? configure = default)
        where TFilter : class, IFilterMetadata
    {
        // 注册过滤器
        services.TryAddScoped<TFilter>();

        // 添加到 MVC 过滤器管道
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(typeof(TFilter));
            configure?.Invoke(options);
        });

        return services;
    }
}
