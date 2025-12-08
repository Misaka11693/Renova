using Renova.Core.Common;

namespace Renova.Core.Components.Security.Authentication.Options;

/// <summary>
/// JWT 配置选项
/// </summary>
public class JwtTokenOptions : IConfigSectionProvider
{
    public static string SectionName => "JwtTokenOptions";

    /// <summary>
    /// AccessToken 配置
    /// </summary>
    public TokenOptions AccessToken { get; set; } = new();

    /// <summary>
    /// RefreshToken 配置
    /// </summary>
    public TokenOptions RefreshToken { get; set; } = new();
}

/// <summary>
/// Token 配置
/// </summary>
public class TokenOptions
{
    /// <summary>
    /// 签发者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 接收者
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 签名密钥
    /// </summary>
    public string SecurityKey { get; set; } = string.Empty;

    /// <summary>
    /// 有效期（分钟）
    /// </summary>
    public int ExpiresMinutes { get; set; }
}
