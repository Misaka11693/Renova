namespace Renova.Rbac.Roles.Models;

/// <summary>
/// 分配角色权限请求参数。
/// </summary>
public class AssignRolePermissionsInput
{
    /// <summary>
    /// 角色 Id。
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 权限 Id 集合。
    /// </summary>
    public List<long> PermissionIds { get; set; } = new();
}
