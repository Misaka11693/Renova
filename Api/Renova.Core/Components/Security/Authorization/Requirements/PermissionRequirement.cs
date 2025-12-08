using Microsoft.AspNetCore.Authorization;

namespace Renova.Core.Components.Security.Authorization.Requirements;

/// <summary>
/// 权限要求（用于 Policy 验证）
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>权限标识</summary>
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
