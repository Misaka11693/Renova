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
    /// </summary>
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);

    }
}
