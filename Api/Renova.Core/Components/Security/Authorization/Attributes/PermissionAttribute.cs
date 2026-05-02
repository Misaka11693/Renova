using Microsoft.AspNetCore.Authorization;

namespace Renova.Core.Components.Security.Authorization.Attributes;

//[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
//public class PermissionAttribute : AuthorizeAttribute
//{
//    /// <summary>
//    /// 构造函数
//    /// </summary>
//    /// <param name="permission"></param>
//    public PermissionAttribute(string permission)
//    {
//        Policy = permission;
//    }
//}

/// <summary>
/// 权限特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class PermissionAttribute : Attribute
{
    /// <summary>
    /// 权限编码
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="code">权限编码</param>
    public PermissionAttribute(string code)
    {
        Code = code;
    }
}