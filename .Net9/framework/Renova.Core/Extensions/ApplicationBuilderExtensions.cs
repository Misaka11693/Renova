using Microsoft.AspNetCore.Builder;
using Renova.Core.Middleware;

namespace Renova.Core.Extensions;

/// <summary>
/// IApplicationBuilder 扩展方法。
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 启用全局异常处理中间件
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns>应用构建器实例</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    /// <summary>
    /// 启用统一 API 响应状态码处理中间件
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns>应用构建器实例</returns>
    public static IApplicationBuilder UseApiResponseStatusHandling(this IApplicationBuilder app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        return app.UseMiddleware<ApiResponseStatusMiddleware>();
    }
}