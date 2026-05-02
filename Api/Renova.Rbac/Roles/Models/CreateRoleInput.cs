namespace Renova.Rbac.Roles.Models;

/// <summary>
/// 创建角色请求参数。
/// </summary>
public class CreateRoleInput
{
    /// <summary>
    /// 角色编码。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 角色名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色状态。
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 排序值。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }
}
