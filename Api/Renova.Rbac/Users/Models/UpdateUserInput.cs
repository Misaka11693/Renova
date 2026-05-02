namespace Renova.Rbac.Users.Models;

/// <summary>
/// 更新用户请求参数。
/// </summary>
public class UpdateUserInput
{
    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户昵称。
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 用户状态。
    /// </summary>
    public int Status { get; set; } = (int)CommonStatus.Enabled;
}
