using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Renova.Security.Authentication.Abstractions;
using Renova.Security.Authentication.Extensions;
using Renova.Security.Authentication.Options;
using Renova.Security.Authentication.Services;
using Renova.Security.Authorization.Handlers;
using Renova.Security.Authorization.Policies;

namespace Renova.Security.Extensions;

/// <summary>
/// 安全模块注册扩展方法
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecuritySetup(this IServiceCollection services)
    {
        // 注册选项
        services.AddOptions<JwtTokenOptions>()
            .BindConfiguration(JwtTokenOptions.SectionName)
            .ValidateDataAnnotations();


        // 1.注册 JWT 认证服务
        services.AddJwtAuthentication();

        // 2.注册 JWT Token 生成服务
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        //  3.注册自定义授权策略提供器
        services.AddSingleton<IAuthorizationPolicyProvider, AppAuthorizationPolicyProvider>();

        //  4.注册自定义权限处理器
        services.AddScoped<IAuthorizationHandler, AppPermissionHandler>();

        // 5. 全局授权策略配置
        //    使用 AuthorizeFilter 会导致当方法上存在 [Authorize] 时触发两次鉴权
        //    因此改用 FallbackPolicy 来实现全局授权
        services.AddAuthorization(options =>
        {
            // 默认策略：要求用户已通过认证
            // 当控制器/方法没有显式标记 [Authorize] 或 [AllowAnonymous] 时，会应用此策略
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        // 全局授权
        // 注释原因：当方法上存在 [Authorize] 时，会触发两次鉴权操作
        //services.Configure<MvcOptions>(options =>
        //{
        //    options.Filters.Add(new AuthorizeFilter());
        //});

        return services;
    }
}
