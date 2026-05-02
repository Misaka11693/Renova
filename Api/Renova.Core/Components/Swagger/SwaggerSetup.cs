using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;


//using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Renova.Core.Components.Swagger;

public static class SwaggerSetup
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
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGroupConfiguration>();

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
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "输入 Token（无需加 Bearer）",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        //var s = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" };
        //var scheme = new OpenApiSecurityScheme
        //{
        //    Reference = new OpenApiReference
        //    {
        //        Type = ReferenceType.SecurityScheme,//注释： 
        //        Id = "JwtBearer"
        //    }
        //};

        //options.AddSecurityRequirement(new OpenApiSecurityRequirement
        //{
        //    [scheme] = Array.Empty<string>()
        //});

        //var securitySchema = new OpenApiSecurityScheme
        //{
        //    Name = "Authorization",
        //    Type = SecuritySchemeType.Http,
        //    Scheme = "Bearer",
        //    BearerFormat = "JWT",
        //    In = ParameterLocation.Header,
        //    Description = "在下框中输入请求头中需要添加Jwt授权Token(无需添加Bearer)",
        //};

        //options.AddSecurityDefinition("JwtBearer", securitySchema);
        //var securityRequirement = new OpenApiSecurityRequirement { { securitySchema, new[] { "Bearer" } } };
        //options.AddSecurityRequirement(securityRequirement);

    }
}

