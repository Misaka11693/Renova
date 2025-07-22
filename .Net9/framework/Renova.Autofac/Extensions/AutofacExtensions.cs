using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Renova.Core;
using System.Diagnostics;


namespace Renova.Autofac;

/// <summary>
/// Autofac容器扩展
/// </summary>
public static class AutofacExtensions
{
    /// <summary>
    /// 使用Autofac作为容器
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <returns></returns>
    public static IHostBuilder UseAutofac(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        hostBuilder.ConfigureContainer<ContainerBuilder>(container =>
        {
            //container.RegisterModule<AutofacModule>();

            var moduleTypes = App.EffectiveTypes.Where(t => typeof(Module).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.IsClass);
            //Log.Information("发现 {ModuleCount} 个待注册模块", moduleTypes.Count());

            var totalSw = Stopwatch.StartNew();
            foreach (var moduleType in moduleTypes)
            {
                var sw = Stopwatch.StartNew();
                var module = (Module)Activator.CreateInstance(moduleType)!;
                container.RegisterModule(module);
                sw.Stop();

                //Log.Information("注册模块完成: {ModuleName} ({Assembly}) 耗时 {ElapsedMs}ms",
                //    moduleType.Name,
                //    moduleType.Assembly.GetName().Name,
                //    sw.ElapsedMilliseconds);
            }
            totalSw.Stop();

            //Log.Information("所有模块注册完成，总耗时 {TotalElapsedMs}ms", totalSw.ElapsedMilliseconds);
        });

        return hostBuilder;
    }
}
