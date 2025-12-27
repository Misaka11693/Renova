using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Renova.Core.Components.Job;

/// <summary>
/// job中间件扩展
/// </summary>
public static class JobMiddleware
{
    /// <summary>
    /// 配置Hangfire仪表盘
    /// </summary>
    public static IApplicationBuilder UseHangfireJobMiddleware(this IApplicationBuilder app)
    {
        app.UseHangfireDashboard("/hangfire");

        return app;
    }
}
