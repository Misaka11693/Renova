using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Renova.Core.Components.Security.Authorization.Attributes;
using Renova.Core.Components.Security.Authorization.Requirements;

namespace Renova.Core.Components.Security.Authorization.Handlers;

/// <summary>
/// 权限授权处理器
/// 职责：
/// 1. 自动解析当前接口所需权限
/// 2. 从用户 Claims 中读取权限
/// 3. 判断是否具备访问权限
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PermissionHandler(
        ILogger<PermissionHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _webHostEnvironment = webHostEnvironment;
    }


    /// <summary>
    /// 授权处理逻辑
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Missing HTTP context");

        // 1. AllowAnonymous 检查
        // 不需要在此处手动处理 AllowAnonymous，ASP.NET Core 的授权中间件会自动跳过带有该特性的端点。
        //if (httpContext.GetEndpoint()?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
        //{
        //    context.Succeed(requirement); // 放行匿名访问
        //    return Task.CompletedTask;
        //}

        // 2. 检查用户是否已登录（未认证用户直接拒绝访问）
        if (httpContext.User.Identity is null || !httpContext.User.Identity.IsAuthenticated)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var endpoint = httpContext.GetEndpoint();
        if (endpoint == null)
            return Task.CompletedTask;

        var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor == null)
            return Task.CompletedTask;

        string permission;

        // 3. 优先使用手动声明的权限
        var permissionAttr = endpoint.Metadata.GetMetadata<PermissionAttribute>();
        if (permissionAttr != null)
        {
            permission = permissionAttr.Code;
        }
        else
        {
            // 4. 自动推导权限（Controller + Action）
            var controller = actionDescriptor.ControllerName.ToLowerInvariant();
            var actionName = actionDescriptor.ActionName;

            // 去掉 Async 后缀
            if (!string.IsNullOrEmpty(actionName) &&
                actionName.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
            {
                actionName = actionName.Substring(0, actionName.Length - 5);
            }

            var action = actionName.ToLowerInvariant();

            permission = $"{controller}:{action}";
        }

        //  5. 获取用户权限
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value);

        //  6. 权限匹配
        if (userPermissions.Contains(permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;

    }
}
