using Microsoft.AspNetCore.Authorization;

namespace Renova.Core.Components.Security.Authorization.Requirements;

/// <summary>
/// 权限授权需求（标记用）
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
}




///// <summary>
///// 权限授权需求
///// </summary>
//public class PermissionRequirement : IAuthorizationRequirement
//{
//    /// <summary>权限标识</summary>
//    public string Permission { get; }

//    public PermissionRequirement(string permission)
//    {
//        Permission = permission;
//    }
//}
