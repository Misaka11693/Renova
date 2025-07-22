using Microsoft.AspNetCore.Builder;
using Renova.Core.Apps.Internal;


namespace Renova.Core.Apps;

/// <summary>
/// 内部 App 副本扩展方法
/// </summary>
public static class InternalAppExtensions
{
    /// <summary>
    /// 配置应用服务和环境
    /// </summary>
    /// <param name="builder">WebApplicationBuilder</param>
    public static void ConfigureApplication(this WebApplicationBuilder builder)
    {
        // 存储服务提供器
        InternalApp.InternalServices = builder.Services;

        // 存储配置对象
        InternalApp.Configuration = builder.Configuration;

        // 存储Web主机环境
        InternalApp.WebHostEnvironment = builder.Environment;
    }

    /// <summary>
    /// 配置中间件
    /// </summary>
    /// <param name="app">WebApplication</param>
    public static void UseApplication(this IApplicationBuilder app)
    {
        // 存储根服务提供器
        InternalApp.RootServices ??= app.ApplicationServices;

        // 添加中间件，在每次请求结束时释放未托管的服务提供器
        app.Use(async (context, next) =>
        {
            try
            {
                // 调用后续中间件处理请求
                await next();
            }
            finally
            {
                // 在请求结束时释放未托管的服务提供器
                App.DisposeUnmanagedObjects();
            }
        });
    }
}
