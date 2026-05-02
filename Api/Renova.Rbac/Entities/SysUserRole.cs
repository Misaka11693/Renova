namespace Renova.Rbac.Entities;

/// <summary>
/// 用户与角色关联实体。
/// </summary>
[SysTable]
[SugarTable("sys_user_role", "用户角色关联表")]
public class SysUserRole : PrimaryKeyEntity
{
    /// <summary>
    /// 用户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", IsNullable = false)]
    public long UserId { get; set; }

    /// <summary>
    /// 角色 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", IsNullable = false)]
    public long RoleId { get; set; }
}
