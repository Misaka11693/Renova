using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;
using Renova.Core.Components.DependencyInjection.Attributes;
using System.Reflection;

namespace Renova.Core.Components;

/// <summary>
/// 依赖注入拓展类
/// </summary>
public static class DependencyInjectionServiceCollectionExtensions
{
    /// <summary>
    /// 添加基于约定和特性的自动依赖注入服务注册
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        return services.AddAutoRegisteredServices();
    }

    /// <summary>
    /// 自动注册符合约定的类型（通过接口标记或 <see cref="DependencyAttribute"/> 特性）
    /// </summary>
    private static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services)
    {
        var injectTypes = App.EffectiveTypes
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                !t.IsInterface &&
                (
                    typeof(ITransientDependency).IsAssignableFrom(t) ||
                    typeof(IScopedDependency).IsAssignableFrom(t) ||
                    typeof(ISingletonDependency).IsAssignableFrom(t) ||
                    t.GetCustomAttribute<DependencyAttribute>() != null
                ));

        foreach (var type in injectTypes)
        {
            ServiceLifetime lifetime;

            var attribute = type.GetCustomAttribute<DependencyAttribute>();
            if (attribute != null)
            {
                lifetime = attribute.Lifetime;
            }
            else if (typeof(ITransientDependency).IsAssignableFrom(type))
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
                    $"类型 {type.FullName} 未通过 [Dependency] 特性或 I*Dependency 接口指定有效的生命周期。");
            }

            var serviceTypes = type.GetInterfaces()
                .Where(i =>
                    // 排除系统/生命周期标记接口
                    !typeof(IDisposable).IsAssignableFrom(i) &&
                    !typeof(IAsyncDisposable).IsAssignableFrom(i) &&
                    !typeof(ITransientDependency).IsAssignableFrom(i) &&
                    !typeof(IScopedDependency).IsAssignableFrom(i) &&
                    !typeof(ISingletonDependency).IsAssignableFrom(i) &&

                    // 必须是标准业务接口命名
                    i.Name.StartsWith("I") &&
                    (
                        i.Name.EndsWith("Service") ||
                        i.Name.EndsWith("Provider") ||
                        i.Name.EndsWith("Dependency") ||
                        i.Name == "I" + type.Name
                    ))
                .ToList();

            if (serviceTypes.Count == 0)
            {
                // 无匹配接口，注册自身
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }
            else
            {
                // 注册所有匹配的服务接口
                foreach (var serviceType in serviceTypes)
                {
                    services.Add(new ServiceDescriptor(serviceType, type, lifetime));
                }
            }
        }

        return services;
    }
}