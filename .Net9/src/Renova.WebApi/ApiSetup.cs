using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Renova.Core.Apps.Extensions;
using Renova.Core.Components;
using Renova.Core.Components.Cache;
using Renova.Core.Components.Cors;
using Renova.Core.Components.EventBus;
using Renova.Core.Components.FileStorage;
using Renova.Core.Components.Job;
using Renova.Core.Components.Localization;
using Renova.Core.Components.Response;
using Renova.Core.Components.Security.Extensions;
using Renova.Core.Components.Swagger;
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
        //builder.Logging.ClearProviders();//移除已经注册的其他日志处理程序

        // 配置应用,优先级最高
        builder.ConfigureApplication();

        // 配置依赖注入
        builder.Services.AddDependencyInjection();

        // Add services to the container.
        builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();// 首字母小写（驼峰样式）
            options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";// 时间格式化
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;// 忽略循环引用
        }).AddApiResponseProvider();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // 配置统一 API 响应过滤器
        builder.Services.AddApiResponseFilter();

        // 全局异常处理（.NET 8 新增功能，已注释）
        // 注释原因：
        // 1. 日志重复：同一个异常会被记录两次 —— 
        //    一次由 ASP.NET Core 内置的 ExceptionHandlerMiddleware 自动记录（app.UseExceptionHandler() .NET 8新增），
        //    另一次由自定义的 Renova.Core.GlobalExceptionHandler 手动记录；
        // 2. 异常重复抛出：在 Visual Studio 中调试时，抛出异常后需点击“继续”两次，
        //    因为异常先在控制器抛出，又被异常处理管道重新触发，导致调试器中断两次；
        //
        // 应确保异常仅捕获一次、日志仅记录一次、响应仅写入一次，避免上述问题，故不使用.NET8新增的全局异常处理功能
        // builder.Services.AddProblemDetails();
        //builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // 配置动态 API 服务
        builder.Services.AddDynamicWebApi();

        // 配置 Swagger（如果前端没有获取到最新swagger.json文件，可以通过清除浏览器的缓存来解决，快捷键为 Ctrl + Shift + R）
        builder.Services.AddSwaggerSetup();

        // 配置缓存服务
        builder.Services.AddCache();

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
        builder.Services.AddLocalizationSetup();

        // 配置安全认证服务
        builder.Services.AddSecuritySetup();

        // 配置缓存服务
        builder.Services.AddCache();

        // 配置定时任务
        builder.Services.AddHangfireJob();

        // Ngix 代理时，获取客户端真实 IP
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

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

            app.UseSwaggerMiddleware();

            app.UseDeveloperExceptionPage();
        }

        // .NET的异常处理中间件，自定义的全局异常处理器替代，中间件源码：
        //https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/Diagnostics/src/ExceptionHandler/ExceptionHandlerExtensions.cs
        //app.UseExceptionHandler();

        // 全局异常处理中间件
        app.UseGlobalExceptionHandling();

        // api 响应状态码处理中间件
        app.UseApiResponseStatusHandling();

        // 使用转发头中间件
        app.UseForwardedHeaders();

        app.UseApplication();

        // hangfire定时任务
        app.UseHangfireJobMiddleware();

        // 本地化中间件
        app.UseRequestLocalizationMiddewar();

        // 浏览器输入 wwwroot 目录下的文件可以直接访问
        app.UseStaticFiles();

        app.UseHttpsRedirection();

        // 认证
        app.UseAuthentication();

        // 授权
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
