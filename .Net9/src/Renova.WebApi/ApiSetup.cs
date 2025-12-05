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
using Renova.Core.Components.SqlSugar;
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
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        // 配置应用服务,优先级最高
        builder.ConfigureApplication();

        // 按契约注入服务
        builder.Services.AddDependencyInjection();

        // 配置控制器及 NewtonsoftJson 设置
        builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            // 首字母小写（驼峰样式）
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            // 时间格式化
            options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            // 忽略循环引用
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // 注册 API 统一响应格式过滤器
        builder.Services.AddApiResponseFilter();

        // 注册动态 WebApi 服务
        builder.Services.AddDynamicWebApi();

        // 注册 Swagger 文档服务（如果前端没有获取到最新swagger.json文件，可以通过清除浏览器的缓存来解决，快捷键为 Ctrl + Shift + R）
        builder.Services.AddSwaggerSetup();

        // 注册事件总线服务
        builder.Services.AddEventBusSetup();

        // 注册文件存储服务
        builder.Services.AddFileStorageSetup();

        // 注册 HttpContext 访问器服务(用于构造函数注入 HttpContext 对象)
        builder.Services.AddHttpContextAccessor();

        // 注册 HttpClient 服务
        builder.Services.AddHttpClient();

        // 注册 CORS 跨域服务
        builder.Services.AddCorsSetup();

        // 注册本地化服务
        builder.Services.AddLocalizationSetup();

        // 注册安全认证服务
        builder.Services.AddSecuritySetup();

        // 注册缓存服务
        builder.Services.AddCache();

        // 注册 SqlSugar 服务
        //builder.Services.AddSqlSugar();

        // 注册 Hangfire 定时任务服务
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
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // 启用 OpenAPI 中间件
            app.MapOpenApi();

            // 启用 Swagger 中间件
            app.UseSwaggerMiddleware();

            // 开发环境异常页面
            app.UseDeveloperExceptionPage();
        }

        // 全局异常处理中间件
        app.UseGlobalExceptionHandling();

        // 统一 API 响应状态码处理中间件
        app.UseApiResponseStatusHandling();

        // 使用 Nginx 代理时，获取客户端真实 IP 中间件
        app.UseForwardedHeaders();

        // 配置应用中间件
        app.UseApplication();

        // Hangfire 定时任务中间件
        app.UseHangfireJobMiddleware();

        // 本地化中间件
        app.UseRequestLocalizationMiddewar();

        // 浏览器输入 wwwroot 目录下的文件可以直接访问
        app.UseStaticFiles();

        // 重定向到 HTTPS
        app.UseHttpsRedirection();

        // 认证
        app.UseAuthentication();

        // 授权
        app.UseAuthorization();

        // 映射控制器路由
        app.MapControllers();

        return app;
    }
}
