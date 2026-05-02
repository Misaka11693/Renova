namespace Renova.Rbac.Metadata;

/// <summary>
/// 描述一条内置权限定义。
/// </summary>
public class RbacPermissionDefinition
{
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
    /// 排序值。
    /// </summary>
    public int Sort { get; set; }
}
