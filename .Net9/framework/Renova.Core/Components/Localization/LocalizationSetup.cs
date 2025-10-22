using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;

namespace Renova.Core.Components.Localization;

public static class LocalizationSetup
{
    public static IServiceCollection AddLocalizationSetup(this IServiceCollection services)
    {
        // 注册选项
        services.AddOptions<LocalizationOptions>()
            .BindConfiguration(LocalizationOptions.SectionName)
            .ValidateDataAnnotations();

        // 获取配置选项
        var options = App.GetOptions<LocalizationOptions>();

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
}