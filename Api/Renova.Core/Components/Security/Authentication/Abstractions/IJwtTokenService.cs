using System.Security.Claims;

namespace Renova.Core.Components.Security.Authentication.Abstractions;

/// <summary>
/// 定义 JWT Token 生成与验证的契约
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// 生成访问令牌
    /// </summary>
    /// <param name="claims">用户声明集合</param>
    string GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <param name="claims">用户声明集合</param>
    string GenerateRefreshToken(IEnumerable<Claim> claims);
}
