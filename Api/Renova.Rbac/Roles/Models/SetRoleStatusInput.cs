namespace Renova.Rbac.Roles.Models;

/// <summary>
/// 设置角色状态请求参数。
/// </summary>
public class SetRoleStatusInput
{
    /// <summary>
    /// 角色 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 角色状态。
    /// </summary>
    public int Status { get; set; }
}
