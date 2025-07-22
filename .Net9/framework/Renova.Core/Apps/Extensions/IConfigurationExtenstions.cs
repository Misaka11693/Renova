using Microsoft.Extensions.Configuration;
using Renova.Core.Apps.Internal;

namespace Renova.Core.Apps.Extensions;

/// <summary>
/// 配置对象扩展方法
/// </summary>
public static class IConfigurationExtenstions
{
    /// <summary>
    /// 刷新配置对象
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IConfiguration? Reload(this IConfiguration configuration)
    {
        if (App.RootServices == null) return configuration;

        var newConfiguration = App.GetService<IConfiguration>(App.RootServices);

        InternalApp.Configuration = newConfiguration;

        return newConfiguration;
    }
}
