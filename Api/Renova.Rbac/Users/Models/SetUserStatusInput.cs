namespace Renova.Rbac.Users.Models;

/// <summary>
/// 设置用户状态请求参数。
/// </summary>
public class SetUserStatusInput
{
    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户状态。
    /// </summary>
    public int Status { get; set; }
}
