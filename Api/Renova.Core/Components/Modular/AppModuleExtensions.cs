using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;

namespace Renova.Core.Components.Modular;

/// <summary>
/// 应用模块化配置扩展方法
/// </summary>
public static class AppModuleExtensions
{
    /// <summary>
    /// 添加模块化配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationModules(this IServiceCollection services)
    {
        var moduleTypes = App.EffectiveTypes.Where(IsAppModule);
        foreach (var moduleType in moduleTypes)
        {
            if (Activator.CreateInstance(moduleType) is IAppModule module)
            {
                module.ConfigServices(services);
            }
        }
        return services;
    }

    /// <summary>
    /// 配置模块化的请求处理管道
    /// </summary>
    public static IApplicationBuilder UseApplicationModules(this IApplicationBuilder app)
    {
        var moduleTypes = App.EffectiveTypes.Where(IsAppModule);
        foreach (var moduleType in moduleTypes)
        {
            if (Activator.CreateInstance(moduleType) is IAppModule module)
            {
                module.ConfigureApp(app);
            }
        }

        return app;
    }

    /// <summary>
    /// 判断是否实现了模块接口
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsAppModule(Type type)
    {
        return type is
        {
            IsClass: true,
            IsAbstract: false,
            IsGenericType: false
        }
        && typeof(IAppModule).IsAssignableFrom(type);
    }
}
