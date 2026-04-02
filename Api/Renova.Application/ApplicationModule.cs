using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Components.Modular;

namespace Renova.Application;

public class ApplicationModule : IAppModule
{
    /// <summary>
    /// 配置服务
    /// </summary>
    public void ConfigServices(IServiceCollection services)
    {
    }

    /// <summary>
    /// 配置app应用
    /// </summary>
    public void ConfigureApp(IApplicationBuilder app)
    {
    }
}
