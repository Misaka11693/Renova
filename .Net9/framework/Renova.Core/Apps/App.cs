using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Renova.Core.Apps.Extensions;
using Renova.Core.Apps.Internal;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Claims;

namespace Renova.Core;

/// <summary>
/// 全局应用类
/// </summary>
public static class App
{
    static App()
    {
        // 未托管的对象
        UnmanagedObjects = new ConcurrentBag<IDisposable>();

        // 获取应用有效程序集
        Assemblies = GetAssemblies();

        // 获取有效的类型集合
        EffectiveTypes = Assemblies.SelectMany(GetTypes);
    }

    /// <summary>
    /// 全局配置选项
    /// </summary>
    public static IConfiguration? Configuration => CatchOrDefault(() => InternalApp.Configuration?.Reload(), new ConfigurationBuilder().Build());

    /// <summary>
    /// 获取Web主机环境，如，是否是开发环境，生产环境等
    /// </summary>
    public static IWebHostEnvironment? WebHostEnvironment => InternalApp.WebHostEnvironment;

    /// <summary>
    /// 存储根服务，可能为空
    /// </summary>
    public static IServiceProvider? RootServices => InternalApp.RootServices;

    /// <summary>
    /// 获取请求上下文
    /// </summary>
    public static HttpContext? HttpContext => CatchOrDefault(() => RootServices?.GetService<IHttpContextAccessor>()?.HttpContext);

    /// <summary>
    /// 获取请求上下文用户
    /// </summary>
    /// <remarks>只有授权访问的页面或接口才存在值，否则为 null</remarks>
    public static ClaimsPrincipal? User => HttpContext?.User;

    /// <summary>
    /// 获取当前请求的文化信息
    /// </summary>
    public static CultureInfo CurrentCulture =>
        GetCurrentCultureFeature()?.Culture ?? CultureInfo.CurrentCulture;

    /// <summary>
    /// 获取当前请求的UI文化信息
    /// </summary>
    public static CultureInfo CurrentUICulture =>
        GetCurrentCultureFeature()?.UICulture ?? CultureInfo.CurrentUICulture;

    /// <summary>
    /// 获取当前文化的名称（如 "zh-CN"）
    /// </summary>
    public static string CurrentCultureName => CurrentCulture.Name;

    /// <summary>
    /// 获取当前UI文化的名称（如 "en-US"）
    /// </summary>
    public static string CurrentUICultureName => CurrentUICulture.Name;

    /// <summary>
    /// 内部方法：安全获取当前请求的文化特性
    /// </summary>
    private static RequestCulture? GetCurrentCultureFeature()
    {
        try
        {
            var feature = HttpContext?.Features?.Get<IRequestCultureFeature>();
            return feature?.RequestCulture;
        }
        catch
        {
            // 防止在非HTTP环境中访问时出错
            return null;
        }
    }

    /// <summary>
    /// 应用有效程序集
    /// </summary>
    public static readonly IEnumerable<Assembly> Assemblies;

    /// <summary>
    /// 有效程序集类型
    /// </summary>
    public static readonly IEnumerable<Type> EffectiveTypes;

    /// <summary>
    /// 未托管的对象集合
    /// </summary>
    public static readonly ConcurrentBag<IDisposable> UnmanagedObjects;

    /// <summary>
    ///  解析服务提供器
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static IServiceProvider? GetServiceProvider(Type serviceType)
    {
        // 解析服务提供器
        if (WebHostEnvironment == default) return RootServices;

        // 第一选择，判断是否是单例注册且单例服务不为空，如果是直接返回根服务提供器
        if (RootServices != null &&
            InternalApp.InternalServices != null &&
            InternalApp.InternalServices.Where((u) => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType))
                                        .Any((u) => u.Lifetime == ServiceLifetime.Singleton))
        {
            return RootServices;
        }

        // 第二选择是获取 HttpContext 对象的 RequestServices
        HttpContext? httpContext = HttpContext;
        if (httpContext?.RequestServices != null)
        {
            return httpContext.RequestServices;
        }

        // 第三选择，创建新的作用域并返回服务提供器
        if (RootServices != null)
        {
            IServiceScope serviceScope = RootServices.CreateScope();
            UnmanagedObjects.Add(serviceScope);
            return serviceScope.ServiceProvider;
        }

        // 第四选择，构建新的服务对象（性能最差）
        ServiceProvider? serviceProvider = InternalApp.InternalServices?.BuildServiceProvider();
        if (serviceProvider != null)
        {
            UnmanagedObjects.Add(serviceProvider);
        }

        return serviceProvider;
    }

    /// <summary>
    /// 获取服务注册的生命周期类型
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static ServiceLifetime? GetServiceLifetime(Type serviceType)
    {
        var serviceDescriptor = InternalApp.InternalServices?
            .FirstOrDefault(u => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType));

        return serviceDescriptor?.Lifetime;
    }

    /// <summary>
    /// 获取请求生命周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public static TService? GetService<TService>(IServiceProvider? serviceProvider = null) where TService : class
    {
        return GetService(typeof(TService), serviceProvider) as TService;
    }

    /// <summary>
    /// 获取请求生命周期的服务
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object? GetService(Type type, IServiceProvider? serviceProvider = null)
    {
        return (serviceProvider ?? GetServiceProvider(type))!.GetService(type);
    }

    /// <summary>
    /// 获取请求生命周期的服务（强制要求服务必须存在）
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static TService GetRequiredService<TService>(IServiceProvider? serviceProvider = null) where TService : class
    {
        return (GetRequiredService(typeof(TService), serviceProvider) as TService)!;
    }

    /// <summary>
    /// 获取请求生命周期的服务（强制要求服务必须存在）
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object GetRequiredService(Type type, IServiceProvider? serviceProvider = null)
    {
        return (serviceProvider ?? GetServiceProvider(type))!.GetRequiredService(type);
    }

    /// <summary>
    /// 获取请求生存周期的服务集合
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IEnumerable<TService>? GetServices<TService>(IServiceProvider? serviceProvider = null) where TService : class
    {
        return (serviceProvider ?? GetServiceProvider(typeof(TService)))?.GetServices<TService>();
    }

    /// <summary>
    /// 获取请求生存周期的服务集合
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IEnumerable<object?> GetServices(Type type, IServiceProvider? serviceProvider = default)
    {
        return (serviceProvider ?? GetServiceProvider(type))!.GetServices(type);
    }

    /// <summary>
    /// GC 回收默认间隔
    /// </summary>
    private const int GC_COLLECT_INTERVAL_SECONDS = 5;

    /// <summary>
    /// 最后一次执行GC.Collect()的时间
    /// </summary>
    private static DateTime? LastGCCollectTime { get; set; }

    /// <summary>
    /// 释放所有未托管的对象
    /// </summary>
    public static void DisposeUnmanagedObjects()
    {
        foreach (IDisposable unmanagedObject in UnmanagedObjects)
        {
            try
            {
                unmanagedObject?.Dispose();
            }
            finally
            {
            }
        }

        if (UnmanagedObjects.Any())
        {
            DateTime utcNow = DateTime.UtcNow;
            if (!LastGCCollectTime.HasValue || (utcNow - LastGCCollectTime.Value).TotalSeconds > GC_COLLECT_INTERVAL_SECONDS)
            {
                LastGCCollectTime = utcNow;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        UnmanagedObjects.Clear();
    }

    /// <summary>
    /// 处理获取对象异常问题
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="action">获取对象委托</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>T</returns>
    private static T? CatchOrDefault<T>(Func<T?> action, T? defaultValue = null)
        where T : class
    {
        try
        {
            return action();
        }
        catch
        {
            return defaultValue ?? null;
        }
    }

    /// <summary>
    /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static IList<Assembly> GetAssemblies()
    {
        //// 获取入口程序集
        //var entryAssembly = Assembly.GetEntryAssembly();//Renova.WebApi.dll

        //var dependencyContext = DependencyContext.Default;//单文件夹部署用不了

        // 程序集名称前缀
        var projectAssemblies = new string[] { "Renova." };

        var assemblies = new List<Assembly>();

        var dllFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"); //D:\MyEasyDotNet\EWms\EWms.Api\EWms.Api\bin\Debug\net9.0\

        foreach (var dllFile in dllFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(dllFile);
            if (!projectAssemblies.Any(s => fileName.StartsWith(s))) continue;
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllFile);
                assemblies.Add(assembly);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load assembly: {fileName}. Error: {e.Message}");
            }
        }

        return assemblies;
    }

    /// <summary>
    /// 加载程序集中的所有类型
    /// </summary>
    /// <param name="ass"></param>
    /// <returns></returns>
    private static IEnumerable<Type> GetTypes(Assembly ass)
    {
        var types = Array.Empty<Type>();

        try
        {
            types = ass.GetTypes();
        }
        catch
        {
            Console.WriteLine($"Error load `{ass.FullName}` assembly.");
        }

        return types.Where(u => u.IsPublic);
    }

    #region 获取选项 

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static TOptions GetConfig<TOptions>(string path)
    {
        var section = Configuration?.GetSection(path);
        if (section == null)
        {
            throw new ArgumentException(nameof(section));
        }
        return section.Get<TOptions>() ?? throw new InvalidOperationException($"Failed to get configuration for `{path}`");
    }

    /// <summary>
    /// 通过IOptions接口获取强类型选项（单例，一旦应用启动后配置就不会变）
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类，需包含无参构造函数</typeparam>
    /// <param name="serviceProvider">服务提供者，默认使用根容器</param>
    /// <returns>应用启动时初始化的选项实例</returns>
    /// <remarks>适用于不需要响应配置变更的场景，配置在应用生命周期内保持不变</remarks>
    public static TOptions GetOptions<TOptions>(IServiceProvider? serviceProvider = null) where TOptions : class
    {
        var provider = serviceProvider ?? App.RootServices;
        return App.GetRequiredService<IOptions<TOptions>>(provider).Value;
    }

    /// <summary>
    /// 通过IOptionsMonitor接口获取强类型选项（单例，支持配置热更新，当配置文件修改时，能获取最新的值）
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类，需包含无参构造函数</typeparam>
    /// <param name="serviceProvider">服务提供者，默认使用根容器</param>
    /// <returns>当前最新的选项实例</returns>
    /// <remarks>适用于需要响应配置变更的单例服务，可通过OnChange事件监听配置更新</remarks>
    public static TOptions GetOptionsMonitor<TOptions>(IServiceProvider? serviceProvider = null) where TOptions : class
    {
        var provider = serviceProvider ?? App.RootServices;
        return App.GetRequiredService<IOptionsMonitor<TOptions>>(provider).CurrentValue;
    }

    /// <summary>
    /// 通过IOptionsSnapshot接口获取强类型选项（作用域，请求周期内配置不变，每次请求时会生成一个快照）
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类，需包含无参构造函数</typeparam>
    /// <param name="serviceProvider">必需的作用域服务提供者</param>
    /// <returns>当前作用域内的选项实例</returns>
    /// <exception cref="ArgumentNullException">当serviceProvider为null时抛出</exception>
    /// <remarks>适用于作用域服务（如HTTP请求），每个作用域内配置保持一致性</remarks>
    public static TOptions GetOptionsSnapshot<TOptions>(IServiceProvider serviceProvider) where TOptions : class
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        return App.GetRequiredService<IOptionsSnapshot<TOptions>>(serviceProvider).Value;
    }

    #endregion

    /// <summary>
    /// 获取一段代码执行耗时
    /// </summary>
    /// <param name="action">委托</param>
    /// <returns><see cref="long"/></returns>
    public static long GetExecutionTime(Action action)
    {
        // 空检查
        if (action == null) throw new ArgumentNullException(nameof(action));

        // 计算接口执行时间
        var timeOperation = Stopwatch.StartNew();
        action();
        timeOperation.Stop();
        return timeOperation.ElapsedMilliseconds;
    }
}
