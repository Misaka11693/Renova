namespace Renova.Rbac.Entities;

/// <summary>
/// 权限实体，可表示接口、菜单或按钮权限。
/// </summary>
[SysTable]
[SugarTable("sys_permission", "系统权限表")]
public class SysPermission : EntityBase
{
    /// <summary>
    /// 权限编码。
    /// </summary>
    [SugarColumn(ColumnDescription = "权限编码", Length = 128, IsNullable = false)]
    [Required(ErrorMessage = "权限编码不能为空。")]
    [MaxLength(128, ErrorMessage = "权限编码长度不能超过 128 个字符。")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 权限名称。
    /// </summary>
    [SugarColumn(ColumnDescription = "权限名称", Length = 64, IsNullable = false)]
    [Required(ErrorMessage = "权限名称不能为空。")]
    [MaxLength(64, ErrorMessage = "权限名称长度不能超过 64 个字符。")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 权限类型。
    /// </summary>
    [SugarColumn(ColumnDescription = "权限类型", Length = 32, IsNullable = false)]
    [Required(ErrorMessage = "权限类型不能为空。")]
    [MaxLength(32, ErrorMessage = "权限类型长度不能超过 32 个字符。")]
    public string Type { get; set; } = PermissionTypes.Api;

    /// <summary>
    /// 路由地址。
    /// </summary>
    [SugarColumn(ColumnDescription = "路由地址", Length = 256, IsNullable = true)]
    [MaxLength(256, ErrorMessage = "路由地址长度不能超过 256 个字符。")]
    public string? Path { get; set; }

    /// <summary>
    /// 请求方法。
    /// </summary>
    [SugarColumn(ColumnDescription = "请求方法", Length = 16, IsNullable = true)]
    [MaxLength(16, ErrorMessage = "请求方法长度不能超过 16 个字符。")]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 父级权限 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "父级权限Id", IsNullable = true)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 权限状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "权限状态", IsNullable = false)]
    public int Status { get; set; } = (int)CommonStatus.Enabled;

    /// <summary>
    /// 排序值。
    /// </summary>
    [SugarColumn(ColumnDescription = "排序值", IsNullable = false)]
    public int Sort { get; set; }
}
