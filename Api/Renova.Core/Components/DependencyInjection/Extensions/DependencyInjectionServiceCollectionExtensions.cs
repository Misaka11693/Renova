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
                    // 判断类型是否实现生命周期标记接口
                    t.IsAssignableTo(typeof(ITransientDependency)) ||
                    t.IsAssignableTo(typeof(IScopedDependency)) ||
                    t.IsAssignableTo(typeof(ISingletonDependency)) ||
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
            else if (type.IsAssignableTo(typeof(ITransientDependency)))
            {
                lifetime = ServiceLifetime.Transient;
            }
            else if (type.IsAssignableTo(typeof(IScopedDependency)))
            {
                lifetime = ServiceLifetime.Scoped;
            }
            else if (type.IsAssignableTo(typeof(ISingletonDependency)))
            {
                lifetime = ServiceLifetime.Singleton;
            }
            else
            {
                throw new InvalidOperationException(
                    $"类型 {type.FullName} 未通过 [Dependency] 特性或 I*Dependency 接口指定有效的生命周期。");
            }

            // 筛选有效的服务接口（排除系统接口和生命周期标记接口）
            var serviceTypes = type.GetInterfaces()
                .Where(i =>
                    // 排除系统或基础设施接口
                    !i.IsAssignableTo(typeof(IDisposable)) &&
                    !i.IsAssignableTo(typeof(IAsyncDisposable)) &&
                    !i.IsAssignableTo(typeof(ITransientDependency)) &&
                    !i.IsAssignableTo(typeof(IScopedDependency)) &&
                    !i.IsAssignableTo(typeof(ISingletonDependency)) &&

                    // 仅保留符合业务命名规范的接口
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
                // 无匹配接口，注册自身类型
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }
            else
            {
                // 为每个匹配的接口注册服务
                foreach (var serviceType in serviceTypes)
                {
                    services.Add(new ServiceDescriptor(serviceType, type, lifetime));
                }
            }
        }

        return services;
    }
}