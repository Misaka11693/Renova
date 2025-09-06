using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Renova.Security.Authentication.Abstractions;
using Renova.Security.Authentication.Const;
using Renova.Security.Authentication.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Renova.Security.Authentication.Services;

/// <summary>
/// JWT 令牌服务
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtTokenOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    public JwtTokenService(IOptions<JwtTokenOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 生成访问令牌（AccessToken）
    /// </summary>
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var accessClaims = AppendStandardClaims(claims, AuthSchemes.AccessToken);
        return BuildToken(accessClaims, _options.AccessToken);
    }

    /// <summary>
    /// 生成刷新令牌（RefreshToken）
    /// </summary>
    public string GenerateRefreshToken(IEnumerable<Claim> claims)
    {
        var refreshClaims = AppendStandardClaims(claims, AuthSchemes.RefreshToken);
        return BuildToken(refreshClaims, _options.RefreshToken);
    }

    /// <summary>
    /// 构建 JWT 令牌
    /// </summary>
    private string BuildToken(IEnumerable<Claim> claims, TokenOptions options)
    {
        // 创建签名凭证（使用 HMAC-SHA256）
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecurityKey)),
            SecurityAlgorithms.HmacSha256
        );

        // 创建 JWT 对象
        var token = new JwtSecurityToken(
            issuer: options.Issuer,           // 签发者
            audience: options.Audience,       // 接收者
            claims: claims,                   // 用户声明
            expires: DateTime.UtcNow.AddMinutes(options.ExpiresMinutes), // 过期时间
            signingCredentials: creds         // 签名凭证
        );

        // 序列化为字符串
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 附加标准声明（JTI、token_type）
    /// </summary>
    private static IEnumerable<Claim> AppendStandardClaims(IEnumerable<Claim> claims, string tokenType)
    {
        var claimList = new List<Claim>(claims)
        {
            // JWT 唯一标识
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //令牌类型（AccessToken / RefreshToken）
            new Claim("token_type", tokenType)
        };

        return claimList;
    }
}
