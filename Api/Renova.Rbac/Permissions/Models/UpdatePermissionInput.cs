namespace Renova.Rbac.Permissions.Models;

/// <summary>
/// 更新权限请求参数。
/// </summary>
public class UpdatePermissionInput
{
    /// <summary>
    /// 权限 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 权限名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 权限类型。
    /// </summary>
    public string Type { get; set; } = PermissionTypes.Api;

    /// <summary>
    /// 可选路由地址。
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 可选请求方法。
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 父级权限 Id。
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 权限状态。
    /// </summary>
    public int Status { get; set; } = (int)CommonStatus.Enabled;

    /// <summary>
    /// 排序值。
    /// </summary>
    public int Sort { get; set; }
}
