using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Renova.Core.Components.Security.Authorization.Requirements;

namespace Renova.Core.Components.Security.Authorization.Policies;

/// <summary>
/// 自定义授权策略提供程序（Policy Provider）
/// </summary>
public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    /// <summary>
    /// 根据策略名称动态获取授权策略
    /// 如果走了自定义策略，就不会再去走默认的策略了，及不会再去执行JWT里的授权认证逻辑
    /// </summary>
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);

    }
}
