namespace Renova.Rbac.Permissions.Models;

/// <summary>
/// 权限分页查询参数。
/// </summary>
public class PermissionPageInput
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
    /// 权限编码。
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 权限名称。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 权限类型。
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 权限状态。
    /// </summary>
    public int? Status { get; set; }
}
