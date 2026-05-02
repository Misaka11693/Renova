namespace Renova.Rbac.Permissions.Models;

/// <summary>
/// 设置权限状态请求参数。
/// </summary>
public class SetPermissionStatusInput
{
    /// <summary>
    /// 权限 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 权限状态。
    /// </summary>
    public int Status { get; set; }
}
