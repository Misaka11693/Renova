using Microsoft.Extensions.DependencyInjection;


namespace Renova.Core.Components.DependencyInjection.Attributes;

/// <summary>
/// 用于指定类的依赖注入行为的属性。
/// 此属性不能应用于多个类，也不能被其他类继承。
/// 默认的服务生命周期为瞬态（<see cref="ServiceLifetime.Transient"/>）。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DependencyAttribute : Attribute
{
    /// <summary>
    /// 服务生命周期类型。
    /// 支持的选项包括：
    /// <list type="bullet">
    /// <item><description>瞬态（<see cref="ServiceLifetime.Transient"/>）：每次请求时创建一个新实例。</description></item>
    /// <item><description>作用域（<see cref="ServiceLifetime.Scoped"/>）：在请求范围内共享同一个实例。</description></item>
    /// <item><description>单例（<see cref="ServiceLifetime.Singleton"/>）：在整个应用程序生命周期内共享单个实例。</description></item>
    /// </list>
    /// 默认值为瞬态。
    /// </summary>
    public ServiceLifetime Lifetime { get; private set; }

    /// <summary>
    /// 初始化 <see cref="DependencyAttribute"/> 类的新实例。
    /// </summary>
    /// <param name="lifetime">服务生命周期类型,默认值为瞬态。</param>
    public DependencyAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        Lifetime = lifetime;
    }
}