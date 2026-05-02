namespace Renova.Rbac.Auth.Models;

/// <summary>
/// 当前登录用户信息。
/// </summary>
public class CurrentUserOutput
{
    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string Account { get; set; } = string.Empty;

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
