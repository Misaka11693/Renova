using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;

namespace Renova.Core.Components.Cors;

/// <summary>
/// CORS 配置扩展
/// </summary>
public static class CorsSetup
{
    /// <summary>
    /// 添加跨域配置
    /// </summary>
    public static IServiceCollection AddCorsSetup(this IServiceCollection services, string policyName = "DefaultPolicy", Action<CorsOptions>? setupAction = null)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, builder =>
            {
                var allowCorsArray = App.GetConfig<string[]>("AllowCors")?
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToArray() ?? Array.Empty<string>();

                if (allowCorsArray.Length == 0)
                {
                    return; // 不配置CORS
                }

                if (allowCorsArray.Contains("*"))
                {
                    // 如果需要允许所有来源并支持凭据
                    builder
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
                else
                {
                    builder
                        .WithOrigins(allowCorsArray)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });

        if (setupAction != null)
        {
            services.Configure(setupAction);
        }

        return services;
    }
}
