using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Renova.Core.Apps;
using Renova.Core.Components.Security.Authentication.Const;
using Renova.Core.Components.Security.Authentication.Options;
using System.Text;

namespace Renova.Core.Components.Security.Authentication.Extensions;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// 注册 JWT 认证（ AccessToken 和 RefreshToken 双令牌）
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {

        // 1.获取配置选项
        var jwtTokenOptions = App.GetOptions<JwtTokenOptions>();

        // 2. 配置认证服务
        //    - AccessToken  ：仅用于访问业务接口
        //    - RefreshToken ：仅用于刷新 AccessToken
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AuthSchemes.AccessToken;
            options.DefaultChallengeScheme = AuthSchemes.AccessToken;
        })

        // 3.注册 AccessToken 验证规则
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
        })

        // 4.注册 RefreshToken 验证规则
        //   仅用于刷新 AccessToken 的接口
        //   使用时需要在控制器/方法上指定：
        //   [Authorize(AuthenticationSchemes = AuthSchemes.RefreshToken)]
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
        });

        return services;
    }
}
