using Renova.Core.Apps;
using Simple.DynamicWebApi;

namespace Renova;

public static class ApiSetup
{
    /// <summary>
    /// 配置服务
    /// </summary>
    /// <param name="builder"></param>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        //配置应用,优先级最高
        builder.ConfigureApplication();

        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        //配置动态 API 服务
        builder.Services.AddDynamicWebApi();

        return builder;
    }

    /// <summary>
    /// 配置 HTTP 请求管道
    /// </summary>
    /// <param name="app"></param>
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseApplication();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
