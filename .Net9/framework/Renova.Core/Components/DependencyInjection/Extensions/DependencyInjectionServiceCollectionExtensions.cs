using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;
using Renova.Core.Components.DependencyInjection.Attributes;
//using Serilog;
using System.Diagnostics;
using System.Reflection;

namespace Renova.Core.Components;

/// <summary>
/// 依赖注入拓展类
/// </summary>
public static class DependencyInjectionServiceCollectionExtensions
{
    /// <summary>
    /// 添加依赖注入接口
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        // 添加内部依赖注入扫描拓展
        services.AddInnerDependencyInjection();

        return services;
    }

    /// <summary>
    /// 添加扫描注入
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    private static IServiceCollection AddInnerDependencyInjection(this IServiceCollection services)
    {
        // 查找所有需要依赖注入的类型
        var injectTypes = App.EffectiveTypes
            .Where(t => (typeof(ITransientDependency).IsAssignableFrom(t) ||
                         typeof(IScopedDependency).IsAssignableFrom(t) ||
                         typeof(ISingletonDependency).IsAssignableFrom(t) ||
                         t.GetCustomAttribute<DependencyAttribute>() != null) &&
                         t.IsClass &&
                        !t.IsInterface &&
                        !t.IsAbstract);

        //Log.Information("发现 {ServiceCount} 个待自动注册服务类型", injectTypes.Count());

        var totalSw = Stopwatch.StartNew();
        //如果同时添加接口与特性，则优先使用特性
        foreach (var type in injectTypes)
        {
            ServiceLifetime lifetime;

            var injectionAttribute = type.GetCustomAttribute<DependencyAttribute>();

            // 优先级：特性 > 接口标记
            if (injectionAttribute != null)
            {
                lifetime = injectionAttribute.Lifetime;
            }
            else
            {
                if (typeof(ITransientDependency).IsAssignableFrom(type))
                {
                    lifetime = ServiceLifetime.Transient;
                }
                else if (typeof(IScopedDependency).IsAssignableFrom(type))
                {
                    lifetime = ServiceLifetime.Scoped;
                }
                else if (typeof(ISingletonDependency).IsAssignableFrom(type))
                {
                    lifetime = ServiceLifetime.Singleton;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"类型 {type.FullName} 实现了 IPrivateDependency 但未指定生命周期");
                }
            }

            var serviceTypes = type.GetInterfaces()
                .Where(i =>
                      !i.IsAssignableFrom(typeof(IDisposable)) &&
                      !i.IsAssignableFrom(typeof(IAsyncDisposable)) &&
                      !i.IsAssignableFrom(typeof(ITransientDependency)) &&
                      !i.IsAssignableFrom(typeof(IScopedDependency)) &&
                      !i.IsAssignableFrom(typeof(ISingletonDependency)) &&
                       i.Name.StartsWith("I")
                        //&& (i.Name.EndsWith("Service") || i.Name.EndsWith("Provider") || i.Name.EndsWith("Dependency") || i.Name.EndsWith(type.Name))
                        )
                        .ToList();

            var sw = Stopwatch.StartNew();
            // 如果没有实现服务接口，则注册自身类型
            if (serviceTypes.Count == 0)
            {
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }
            else
            {
                // 注册所有服务接口
                foreach (var serviceType in serviceTypes)
                {
                    services.Add(new ServiceDescriptor(serviceType, type, lifetime));
                }
            }
            sw.Stop();
            LogServiceRegistration(type, lifetime, serviceTypes, sw.ElapsedMilliseconds);
        }
        totalSw.Stop();
        //Log.Information("服务注册完成，共注册 {RegisteredServices} 个服务，总耗时 {TotalElapsedMs}ms", injectTypes.Count(), totalSw.ElapsedMilliseconds);
        return services;
    }

    /// <summary>
    /// 记录服务注册信息
    /// </summary>
    private static void LogServiceRegistration(Type implementationType, ServiceLifetime lifetime, List<Type> serviceTypes, long elapsedMs)
    {
        var lifetimeStr = lifetime switch
        {
            ServiceLifetime.Transient => "Transient",
            ServiceLifetime.Scoped => "Scoped",
            ServiceLifetime.Singleton => "Singleton",
            _ => throw new NotImplementedException(),
        };

        foreach (var serviceType in serviceTypes)
        {
            var serviceTypeName = serviceType == implementationType
                ? "[Self]"
                : serviceType.Name;

            //Log.Information("注册服务: {ServiceType} → {ImplementationType} ({Lifetime}) 耗时 {ElapsedMs}ms",
            //              serviceTypeName,
            //              implementationType.Name,
            //              lifetimeStr,
            //              elapsedMs);
        }
    }
}
