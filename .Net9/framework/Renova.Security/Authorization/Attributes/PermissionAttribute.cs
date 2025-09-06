using Microsoft.AspNetCore.Authorization;

namespace Renova.Security.Authorization.Attributes;

/// <summary>
/// 权限验证特性（基于 Policy 策略）
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class PermissionAttribute : AuthorizeAttribute
{
    public PermissionAttribute(string permission)
    {
        Policy = permission;
    }
}
