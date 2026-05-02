namespace Renova.Rbac.Permissions.Models;

/// <summary>
/// 权限返回结果。
/// </summary>
public class PermissionOutput
{
    /// <summary>
    /// 权限 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 权限编码。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 权限名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 权限类型。
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 可选路由地址。
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 可选请求方法。
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 权限状态。
    /// </summary>
    public int Status { get; set; }
}
