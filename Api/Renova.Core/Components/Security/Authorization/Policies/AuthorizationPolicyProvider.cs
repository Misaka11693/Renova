using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Renova.Core.Components.Security.Authorization.Requirements;

namespace Renova.Core.Components.Security.Authorization.Policies;

/// <summary>
/// 自定义策略提供器
/// 作用：
/// 统一创建 "Permission" 策略，避免在 Program 中重复定义
/// </summary>
public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    /// <summary>
    /// 统一权限策略名称
    /// </summary>
    private const string POLICY_NAME = "Permission";

    /// <summary>
    /// 构造函数
    /// </summary>

    public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    ///// <summary>
    ///// 根据策略名称动态获取授权策略
    ///// </summary>
    ///// <param name="policyName">策略名称</param>
    ///// <returns></returns>
    //public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    //{
    //    if (policyName == POLICY_NAME)
    //    {
    //        var policy = new AuthorizationPolicyBuilder()
    //            .AddRequirements(new PermissionRequirement(policyName))
    //            .Build();

    //        return Task.FromResult<AuthorizationPolicy?>(policy);
    //    }

    //    return base.GetPolicyAsync(policyName);
    //}
}





///// <summary>
///// 自定义授权策略提供程序（Policy Provider）
///// </summary>
//public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
//{
//    public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

//    /// <summary>
//    /// 根据策略名称动态获取授权策略
//    /// 如果走了自定义策略，就不会再去走默认的策略了，及不会再去执行JWT里的授权认证逻辑
//    /// </summary>
//    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
//    {
//        var policy = new AuthorizationPolicyBuilder()
//            .AddRequirements(new PermissionRequirement(policyName))
//            .Build();

//        return Task.FromResult<AuthorizationPolicy?>(policy);

//    }
//}
