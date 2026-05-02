namespace Renova.Rbac.Auth.Models;

/// <summary>
/// 登录请求参数。
/// </summary>
public class LoginInput
{
    /// <summary>
    /// 登录账号。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码。
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
