using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Response;

namespace Renova.Core.Extensions;

/// <summary>
/// 统一 API 响应提供器的注册扩展
/// </summary>
public static class ApiResponseProviderSetup
{
    /// <summary>
    /// 注册默认的统一 API 响应提供器（<see cref="ApiResponseProvider"/>）
    /// </summary>
    public static IServiceCollection AddApiResponseProvider(this IServiceCollection services)
    {
        return services.AddApiResponseProvider<ApiResponseProvider>();
    }

    /// <summary>
    /// 注册指定类型的统一 API 响应提供器
    /// </summary>
    public static IServiceCollection AddApiResponseProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IApiResponseProvider
    {
        services.AddSingleton<IApiResponseProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// 注册默认的统一 API 响应提供器（<see cref="ApiResponseProvider"/>）
    /// </summary>
    public static IMvcBuilder AddApiResponseProvider(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.AddApiResponseProvider<ApiResponseProvider>();
        return mvcBuilder;
    }

    /// <summary>
    /// 注册指定类型的统一 API 响应提供器
    /// </summary>
    public static IMvcBuilder AddApiResponseProvider<TProvider>(this IMvcBuilder mvcBuilder)
        where TProvider : class, IApiResponseProvider
    {
        mvcBuilder.Services.AddApiResponseProvider<TProvider>();
        return mvcBuilder;
    }
}