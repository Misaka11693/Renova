using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Renova.Core.Components.Localization;

/// <summary>
/// 基于 RESX 文件的本地化服务
/// </summary>
public class ResourceLocalizerService : ILocalizationService
{
    private readonly IStringLocalizer _localizer;
    private readonly LocalizationOptions _options;

    public ResourceLocalizerService(
        IStringLocalizerFactory factory,
        IOptions<LocalizationOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ResourceAssemblyName))
            throw new ArgumentException("ResourceAssemblyName 不能为空，请在配置中指定或使用默认值。");

        if (string.IsNullOrWhiteSpace(_options.ResourceFileName))
            throw new ArgumentException("ResourceFileName 不能为空，请在配置中指定或使用默认值。");

        _localizer = factory.Create(_options.ResourceFileName, _options.ResourceAssemblyName);
    }

    //https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/5.0/localization-members-removed
    public string GetString(string key, string? culture = null)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            return GetStringWithCulture(key, culture);
        }
        return _localizer[key];
    }

    private string GetStringWithCulture(string key, string culture)
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            return _localizer[key];
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }
}
