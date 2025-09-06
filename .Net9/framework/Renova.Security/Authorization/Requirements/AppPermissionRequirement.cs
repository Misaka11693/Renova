using Microsoft.AspNetCore.Authorization;

namespace Renova.Security.Authorization.Requirements;

/// <summary>
/// 权限要求（用于 Policy 验证）
/// </summary>
public class AppPermissionRequirement : IAuthorizationRequirement
{
    /// <summary>权限标识</summary>
    public string Permission { get; }

    public AppPermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
