using Microsoft.Extensions.DependencyModel;
using System.Reflection;
using System.Runtime.Loader;

namespace Renova.Core.Common.Helper;

/// <summary>
/// 运行时帮助类
/// </summary>
public class RuntimeHelper
{
    /// <summary>
    /// 获取当前应用程序中所有“项目程序集”
    /// 自动排除系统程序集（如 System.*、Microsoft.* 等）以及通过 NuGet 引用的第三方包。
    /// https://www.cnblogs.com/yanglang/p/6866165.html
    /// </summary>
    public static List<Assembly> GetProjectAssemblies()
    {
        var list = new List<Assembly>();
        var deps = DependencyContext.Default;
        if (deps == null)
        {
            throw new InvalidOperationException("无法获取当前应用程序的依赖上下文。请确保在支持 .NET Core 的环境中运行此代码。");
        }

        // 筛选项目程序集：排除所有的系统程序集、Nuget下载包
        var libs = deps.CompileLibraries.Where(lib => !lib.Serviceable && !string.Equals(lib.Type, "package", StringComparison.OrdinalIgnoreCase));
        foreach (var lib in libs)
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
                list.Add(assembly);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"无法加载程序集 '{lib.Name}'。请确保该程序集存在且可访问。");
            }
        }

        return list;
    }
}
