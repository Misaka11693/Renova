using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Renova.Core;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Renova.Swagger;

public static class SwaggerSetup
{
    public const string AllGroupName = "All";
    public const string DefaultGroupName = "Default";

    /// <summary>
    /// 预注册特殊分组
    /// </summary>
    public static readonly Dictionary<string, OpenApiInfo> ApiDocs = new()
    {
        [AllGroupName] = new OpenApiInfo
        {
            Title = "🌐 全部接口",
            Version = "v1.0.0",
            Description = "系统所有可用接口的完整集合"
        },
        [DefaultGroupName] = new OpenApiInfo
        {
            Title = "⚙️ 默认分组",
            Version = "v1.0.0",
            Description = "未明确指定分组的接口集合"
        }
    };

    /// <summary>
    /// 配置Swagger生成器服务
    /// </summary>
    public static IServiceCollection AddSwaggerSetup(
        this IServiceCollection services,
        Action<SwaggerGenOptions>? setupAction = null)
    {
        services.AddSwaggerGen(
            options =>
            {
                // 应用外部配置
                setupAction?.Invoke(options);

                //包含XML注释文档
                IncludeXmlComments(options);

                //配置JWT认证
                ConfigureJwtAuthentication(options);
            });

        //配置API文档分组
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGroup>();

        return services;
    }

    /// <summary>
    /// 包含XML注释文档
    /// </summary>
    private static void IncludeXmlComments(SwaggerGenOptions options)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        // 获取所有 .xml 文件（默认不包含子目录）
        foreach (var xmlFile in Directory.GetFiles(basePath, "*.xml"))
        {
            options.IncludeXmlComments(xmlFile, true);
        }
    }

    /// <summary>
    /// 配置JWT认证
    /// </summary>
    private static void ConfigureJwtAuthentication(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT授权(数据将在请求头中进行传输) 在下方输入{token} 即可，无需添加Bearer",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer", //必须小写
            BearerFormat = "JWT"
        });

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "JwtBearer"
            }
        };

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [scheme] = Array.Empty<string>()
        });
    }

    /// <summary>
    /// 配置Swagger中间件
    /// </summary>
    /// <param name="app"></param>
    public static void AddSwagger(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            #region 动态加载分组
            // 1. 预注册分组
            foreach (var doc in ApiDocs)
            {
                c.SwaggerEndpoint($"/swagger/{doc.Key}/swagger.json", $"{doc.Value.Title}");
            }

            // 2. 动态加载其他分组（排除预注册分组）
            var provider = App.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            provider.ApiDescriptionGroups.Items
                .Where(g =>
                    !string.IsNullOrEmpty(g.GroupName) &&
                    !ApiDocs.ContainsKey(g.GroupName))
                .OrderBy(g => g.GroupName)
                .ToList()
                .ForEach(group =>
                    c.SwaggerEndpoint(
                        $"/swagger/{group.GroupName}/swagger.json",
                        $"{group.GroupName}"
                    )
                );
            #endregion

            #region UI配置
            //c.RoutePrefix = "swagger";
            c.RoutePrefix = string.Empty; // 设置Swagger UI为默认首页路径
            c.DocExpansion(DocExpansion.List);//折叠所有标签
            c.DefaultModelsExpandDepth(-1); // 隐藏所有模型
            c.DisplayRequestDuration(); // 显示请求持续时间 
            #endregion
        });
    }
}

