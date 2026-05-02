namespace Renova.Rbac.Entities;

/// <summary>
/// 角色与权限关联实体。
/// </summary>
[SysTable]
[SugarTable("sys_role_permission", "角色权限关联表")]
public class SysRolePermission : PrimaryKeyEntity
{
    /// <summary>
    /// 角色 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", IsNullable = false)]
    public long RoleId { get; set; }

    /// <summary>
    /// 权限 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "权限Id", IsNullable = false)]
    public long PermissionId { get; set; }
}
