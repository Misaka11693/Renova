namespace Renova.Core.Components.Security.Contexts;

/// <summary>
/// 当前用户信息。
/// </summary>
public class CurrentUserInfo
{
    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 用户名。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 用户昵称。
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 操作人用户 Id，不存在时返回空。
    /// </summary>
    public long? OperatorUserId => UserId;

    /// <summary>
    /// 操作人名称，优先返回用户名。
    /// </summary>
    public string? OperatorUserName => UserName;
}
