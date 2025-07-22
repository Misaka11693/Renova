using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Renova.Core.Apps.Internal;

/// <summary>
/// 内部 App 副本
/// </summary>
internal static class InternalApp
{
    /// <summary>
    /// 应用服务
    /// </summary>
    internal static IServiceCollection? InternalServices;

    /// <summary>
    /// 根服务
    /// </summary>
    internal static IServiceProvider? RootServices;

    /// <summary>
    /// 配置对象
    /// </summary>
    internal static IConfiguration? Configuration;

    /// <summary>
    /// 获取Web主机环境
    /// </summary>
    internal static IWebHostEnvironment? WebHostEnvironment;
}
