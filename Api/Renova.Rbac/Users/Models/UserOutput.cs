namespace Renova.Rbac.Users.Models;

/// <summary>
/// 用户返回结果。
/// </summary>
public class UserOutput
{
    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称。
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 用户状态。
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 角色编码列表。
    /// </summary>
    public List<string> Roles { get; set; } = new();
}
