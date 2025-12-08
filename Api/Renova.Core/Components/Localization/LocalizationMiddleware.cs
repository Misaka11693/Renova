using Microsoft.AspNetCore.Builder;
using Renova.Core.Apps;

namespace Renova.Core.Components.Localization;

public static class LocalizationMiddleware
{
    /// <summary>
    /// 请求本地化中间件
    /// </summary>
    public static IApplicationBuilder UseRequestLocalizationMiddewar(this IApplicationBuilder app)
    {
        var localizationOptions = App.GetOptions<LocalizationOptions>();

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
