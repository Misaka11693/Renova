using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Renova.Core.Components.Security.Authorization.Attributes;
using Renova.Core.Components.Security.Authorization.Requirements;

namespace Renova.Core.Components.Security.Authorization.Handlers;

/// <summary>
/// 权限授权处理器。
/// </summary>
public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private const string PermissionClaimType = "Permission";

    private readonly ILogger<PermissionHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 初始化 <see cref="PermissionHandler"/> 类的新实例。
    /// </summary>
    /// <param name="logger">日志记录器。</param>
    /// <param name="httpContextAccessor">HTTP 上下文访问器。</param>
    public PermissionHandler(
        ILogger<PermissionHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 处理权限授权要求。
    /// </summary>
    /// <param name="context">授权上下文。</param>
    /// <param name="requirement">权限要求。</param>
    /// <returns>表示异步操作的任务。</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        //不需要在此处手动处理 AllowAnonymous，因为在 ASP.NET Core 中，AllowAnonymous 特性会自动跳过授权处理器的执行。

        // 1. 用户是否已认证
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // 2. 获取当前 HTTP Context
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return Task.CompletedTask;

        // 3. 获取当前 Endpoint
        var endpoint = httpContext.GetEndpoint();
        if (endpoint is null)
            return Task.CompletedTask;

        // 4. 获取 Controller / Action
        var actionDescriptor =
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (actionDescriptor is null)
            return Task.CompletedTask;

        // 5. 获取权限码
        var permissionAttribute =
            endpoint.Metadata.GetMetadata<PermissionAttribute>();

        string permission;

        if (permissionAttribute is not null)
        {
            // 显式声明的权限优先
            permission = permissionAttribute.Code;
        }
        else
        {
            // Controller + Action 自动生成权限
            var controller = actionDescriptor.ControllerName;
            var action = actionDescriptor.ActionName;

            if (action.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
            {
                action = action[..^5];
            }

            permission = $"{controller}:{action}".ToLowerInvariant();
        }

        // 6. 判断用户是否拥有权限
        var hasPermission = context.User.Claims
            .Where(x => x.Type == PermissionClaimType)
            .Any(x => string.Equals(
                x.Value,
                permission,
                StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}