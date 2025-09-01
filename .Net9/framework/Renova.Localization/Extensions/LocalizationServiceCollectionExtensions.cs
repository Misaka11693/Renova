using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core;
using Renova.Localization.Abstractions;
using Renova.Localization.Options;
using Renova.Localization.Services;

namespace Renova.Localization.Extensions;

public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// 本地化服务，根据配置使用 JSON 或 RESX 资源文件
    /// </summary>
    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        // 注册选项
        services.AddOptions<AppLocalizationOptions>()
            .BindConfiguration(AppLocalizationOptions.SectionName)
            .ValidateDataAnnotations();

        // 获取配置选项
        var options = App.GetOptions<AppLocalizationOptions>();

        // 根据配置选择实现
        if (options.ProviderType.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            services.AddJsonLocalization(opt =>
            {
                opt.ResourcesPath = options.ResourcesPath;
                opt.ResourcesType = options.JsonResourcesType;
            });

            services.AddSingleton<ILocalizationService, ResourceLocalizerService>();
        }
        else if (options.ProviderType.Equals("resx", StringComparison.OrdinalIgnoreCase))
        {
            services.AddLocalization(opt =>
            {
                opt.ResourcesPath = options.ResourcesPath;
            });
            services.AddSingleton<ILocalizationService, ResourceLocalizerService>();
        }
        else
        {
            throw new NotSupportedException($"不支持的资源文件格式: {options.ProviderType}");
        }


        return services;
    }

    /// <summary>
    /// 请求本地化中间件
    /// </summary>
    public static IApplicationBuilder UseRequestLocalizationMiddewar(this IApplicationBuilder app)
    {
        var localizationOptions = App.GetOptions<AppLocalizationOptions>();

        // 配置支持的语言和默认语言
        var options = new RequestLocalizationOptions()
            .AddSupportedCultures(localizationOptions.SupportedCultures)
            .AddSupportedUICultures(localizationOptions.SupportedCultures)
            .SetDefaultCulture(localizationOptions.DefaultCulture);

        options.ApplyCurrentCultureToResponseHeaders = true;
        app.UseRequestLocalization(options);

        return app;
    }
}
