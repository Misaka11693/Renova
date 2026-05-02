namespace Renova.Rbac.Roles.Models;

/// <summary>
/// 角色返回结果。
/// </summary>
public class RoleOutput
{
    /// <summary>
    /// 角色 Id。
    /// </summary>
    public long Id { get; set; }

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
    public int Status { get; set; }

    /// <summary>
    /// 权限编码列表。
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}
