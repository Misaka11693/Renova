using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Renova.Security.Authorization.Requirements;

namespace Renova.Security.Authorization.Policies;

/// <summary>
/// 自定义授权策略提供程序（Policy Provider）
/// </summary>
public class AppAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public AppAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    /// <summary>
    /// 根据策略名称动态获取授权策略
    /// </summary>
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new AppPermissionRequirement(policyName))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);

    }
}
