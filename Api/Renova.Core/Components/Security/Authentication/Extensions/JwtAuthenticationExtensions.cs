using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Renova.Core.Apps;
using Renova.Core.Components.Security.Authentication.Const;
using Renova.Core.Components.Security.Authentication.Options;
using System.Text;

namespace Renova.Core.Components.Security.Authentication.Extensions;

/// <summary>
/// JWT 认证扩展方法
/// </summary>
public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// 注册 JWT 认证（使用 AccessToken 和 RefreshToken 双令牌机制）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>配置后的服务集合</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // 1. 获取 JWT 配置选项
        var jwtTokenOptions = App.GetOptions<JwtTokenOptions>();

        // 2. 配置认证服务
        //    - AccessToken  ：用于访问受保护的业务接口
        //    - RefreshToken ：仅用于刷新 AccessToken 的专用接口
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AuthSchemes.AccessToken;
            options.DefaultChallengeScheme = AuthSchemes.AccessToken;
        })

        // 3. 注册 AccessToken 验证规则
        .AddJwtBearer(AuthSchemes.AccessToken, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtTokenOptions.AccessToken.Issuer,
                ValidAudience = jwtTokenOptions.AccessToken.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenOptions.AccessToken.SecurityKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // 1. 验证 token_type 类型
                    var tokenType = context.Principal?.FindFirst(AuthSchemes.TokenType)?.Value;
                    if (!string.Equals(tokenType, AuthSchemes.AccessToken, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("Invalid token_type claim for access token scheme.");
                        return Task.CompletedTask;
                    }

                    // 2. 验证 User-Agent 是否匹配，防止令牌被盗用
                    var tokenUa = context.Principal?.FindFirst("ua")?.Value;
                    var requestUa = context.HttpContext.Request.Headers.UserAgent.ToString();

                    if (!string.Equals(tokenUa?.Trim(), requestUa?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("User-Agent mismatch");
                    }

                    return Task.CompletedTask;
                }
            };
        })

        // 4. 注册 RefreshToken 验证规则
        //    使用时需在控制器/方法上显式指定：
        //    [Authorize(AuthenticationSchemes = AuthSchemes.RefreshToken)]
        .AddJwtBearer(AuthSchemes.RefreshToken, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtTokenOptions.RefreshToken.Issuer,
                ValidAudience = jwtTokenOptions.RefreshToken.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenOptions.RefreshToken.SecurityKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                // 验证 token_type 是否为 refresh，并从自定义请求头中提取 RefreshToken
                OnMessageReceived = context =>
                {
                    var tokenType = context.Request.Headers[AuthSchemes.TokenType].ToString();
                    if (!string.Equals(tokenType, AuthSchemes.RefreshToken, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("Invalid token_type for refresh token scheme.");
                        return Task.CompletedTask;
                    }

                    return Task.CompletedTask;
                },

                // 验证 User-Agent 是否匹配，防止令牌被盗用
                OnTokenValidated = context =>
                {

                    // 1. 验证 token_type 类型
                    var tokenType = context.Principal?.FindFirst("token_type")?.Value;
                    if (!string.Equals(tokenType, AuthSchemes.RefreshToken, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("Invalid token_type claim for refresh token scheme.");
                        return Task.CompletedTask;
                    }

                    // 2. 验证 User-Agent 是否匹配，防止令牌被盗用
                    var tokenUa = context.Principal?.FindFirst("ua")?.Value;
                    var requestUa = context.HttpContext.Request.Headers.UserAgent.ToString();

                    if (!string.Equals(tokenUa?.Trim(), requestUa?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("User-Agent mismatch");
                    }

                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}