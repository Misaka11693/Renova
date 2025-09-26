using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Renova.Core;

/// <summary>
/// 统一 API 响应配置扩展
/// </summary>
public static class ApiResponseSetup
{
    /// <summary>
    /// 添加统一 API 响应过滤器，使用指定的响应提供器实现
    /// </summary>
    public static IServiceCollection AddApiResponseFilter<TApiResponseProvider>(
        this IServiceCollection services,
        Action<MvcOptions>? configure = default)
        where TApiResponseProvider : class, IApiResponseProvider
    {
        // 1.注册响应提供器
        services.AddSingleton<IApiResponseProvider, TApiResponseProvider>();

        // 2.注册过滤器
        services.AddScoped<ApiResponseFilter>();

        // 3.添加到 MVC 过滤器中
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(typeof(ApiResponseFilter));

            // 其他额外配置
            configure?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// 为 MVC 构建器添加统一 API 响应支持
    /// </summary>
    public static IMvcBuilder AddApiResponseFilter<TApiResponseProvider>(
        this IMvcBuilder mvcBuilder,
        Action<MvcOptions>? configure = default)
        where TApiResponseProvider : class, IApiResponseProvider
    {
        // 调用 IServiceCollection 的扩展方法
        mvcBuilder.Services.AddApiResponseFilter<TApiResponseProvider>(configure);
        return mvcBuilder;
    }
}
