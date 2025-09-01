using Renova.Core;
using Renova.Core.Apps;
using Renova.EventBus;
using Renova.FileStorage.Extensions;
using Renova.Localization.Extensions;
using Renova.Swagger;
using Simple.DynamicWebApi;

namespace Renova;

/// <summary>
/// API 配置扩展
/// </summary>
public static class ApiSetup
{
    /// <summary>
    /// 配置服务
    /// </summary>
    /// <param name="builder"></param>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        // 配置过滤器
        builder.Services.AddControllers(option =>
        {
            // Api统一响应格式 
            option.Filters.Add(typeof(ApiResponseFilter));
        });

        // 配置应用,优先级最高
        builder.ConfigureApplication();

        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // 注册全局异常处理器
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // 配置动态 API 服务
        builder.Services.AddDynamicWebApi();

        // 配置 Swagger（如果前端没有获取到最新swagger.json文件，可以通过清除浏览器的缓存来解决，快捷键为 Ctrl + Shift + R）
        builder.Services.AddSwaggerSetup();

        // 配置事件总线
        builder.Services.AddEventBusSetup();

        // 配置文件存储服务
        builder.Services.AddFileStorageSetup();

        // 配置 HttpContext 访问器(用于构造函数注入 HttpContext 对象)
        builder.Services.AddHttpContextAccessor();

        // 配置 HTTP 工厂服务
        builder.Services.AddHttpClient();

        // 配置 CORS
        builder.Services.AddCorsSetup();

        // 配置本地化服务
        builder.Services.AddAppLocalization();

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

            app.UseSwaggerSetup();

            //app.UseDeveloperExceptionPage();
        }

        app.UseExceptionHandler();

        app.UseApplication();

        // 本地化中间件
        app.UseRequestLocalizationMiddewar();

        // 浏览器输入 wwwroot 目录下的文件可以直接访问
        app.UseStaticFiles();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
