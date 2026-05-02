namespace Renova.Rbac.Users.Models;

/// <summary>
/// 创建用户请求参数。
/// </summary>
public class CreateUserInput
{
    /// <summary>
    /// 登录账号。
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码。
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称。
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 用户状态。
    /// </summary>
    public int Status { get; set; } = 1;
}
