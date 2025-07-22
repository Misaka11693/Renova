namespace Renova;

public static class ApiSetup
{
    /// <summary>
    /// 配置服务
    /// </summary>
    /// <param name="builder"></param>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    /// <summary>
    /// 配置 HTTP 请求管道
    /// </summary>
    /// <param name="app"></param>
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        return app;
    }
}
