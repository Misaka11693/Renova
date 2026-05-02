namespace Renova.Rbac.Roles.Models;

/// <summary>
/// 更新角色请求参数。
/// </summary>
public class UpdateRoleInput
{
    /// <summary>
    /// 角色 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 角色名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色状态。
    /// </summary>
    public int Status { get; set; } = (int)CommonStatus.Enabled;

    /// <summary>
    /// 排序值。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }
}
