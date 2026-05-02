namespace Renova.Rbac.Entities;

/// <summary>
/// 系统角色实体。
/// </summary>
[SysTable]
[SugarTable("sys_role", "系统角色表")]
public class SysRole : EntityBase
{
    /// <summary>
    /// 角色编码。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色编码", Length = 64, IsNullable = false)]
    [Required(ErrorMessage = "角色编码不能为空。")]
    [MaxLength(64, ErrorMessage = "角色编码长度不能超过 64 个字符。")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 角色名称。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色名称", Length = 64, IsNullable = false)]
    [Required(ErrorMessage = "角色名称不能为空。")]
    [MaxLength(64, ErrorMessage = "角色名称长度不能超过 64 个字符。")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色状态", IsNullable = false)]
    public int Status { get; set; } = (int)CommonStatus.Enabled;

    /// <summary>
    /// 排序值。
    /// </summary>
    [SugarColumn(ColumnDescription = "排序值", IsNullable = false)]
    public int Sort { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    [MaxLength(256, ErrorMessage = "备注长度不能超过 256 个字符。")]
    public string? Remark { get; set; }
}
