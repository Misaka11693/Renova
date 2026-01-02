namespace Renova.Core.Components.Security.Authentication.Const;

/// <summary>
/// 认证方案名称常量
/// </summary>
public static class AuthSchemes
{
    /// <summary>
    /// 令牌类型(访问令牌、刷新令牌)
    /// </summary>
    public const string TokenType = "TokenType";

    /// <summary>
    /// 访问令牌
    /// </summary>
    public const string AccessToken = "AccessToken";

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public const string RefreshToken = "RefreshToken";
}
