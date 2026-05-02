namespace Renova.Rbac.Users.Models;

/// <summary>
/// 用户分页查询参数。
/// </summary>
public class UserPageInput
{
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每页条数。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 用户昵称。
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 用户状态。
    /// </summary>
    public int? Status { get; set; }
}
