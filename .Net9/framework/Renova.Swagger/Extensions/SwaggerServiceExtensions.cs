using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Renova.Swagger;

public static class SwaggerServiceExtensions
{
    /// <summary>
    /// 配置Swagger生成器服务
    /// </summary>
    public static IServiceCollection AddSwaggerSetup(this IServiceCollection services, Action<SwaggerGenOptions>? setupAction = null)
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
}
