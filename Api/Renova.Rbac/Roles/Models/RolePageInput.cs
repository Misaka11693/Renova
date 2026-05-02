namespace Renova.Rbac.Roles.Models;

/// <summary>
/// 角色分页查询参数。
/// </summary>
public class RolePageInput
{
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每页条数。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 角色编码。
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 角色名称。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 角色状态。
    /// </summary>
    public int? Status { get; set; }
}
