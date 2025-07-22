using Renova.Core.Apps;

namespace Renova;

public static class ApiSetup
{
    /// <summary>
    /// 配置服务
    /// </summary>
    /// <param name="builder"></param>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.ConfigureApplication();//配置应用,优先级最高

        return builder;
    }

    /// <summary>
    /// 配置 HTTP 请求管道
    /// </summary>
    /// <param name="app"></param>
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseApplication();

        return app;
    }
}
