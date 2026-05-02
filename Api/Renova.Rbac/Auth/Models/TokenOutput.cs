namespace Renova.Rbac.Auth.Models;

/// <summary>
/// 令牌返回结果。
/// </summary>
public class TokenOutput
{
    /// <summary>
    /// 访问令牌。
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌。
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称。
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 角色编码列表。
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// 权限编码列表。
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}
