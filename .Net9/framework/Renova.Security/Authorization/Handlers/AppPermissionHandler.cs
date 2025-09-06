using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Renova.Security.Authorization.Requirements;

namespace Renova.Security.Authorization.Handlers;

/// <summary>
/// 策略授权处理器
/// </summary>
public class AppPermissionHandler : AuthorizationHandler<AppPermissionRequirement>
{
    private readonly ILogger<AppPermissionHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="accessor"></param>
    /// <param name="webHostEnvironment"></param>
    public AppPermissionHandler(
        ILogger<AppPermissionHandler> logger,
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
        AppPermissionRequirement requirement)
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

        // 从用户的 Claims 中获取权限
        var permissions = context.User.FindAll("Permission").Select(c => c.Value);

        // 如果用户拥有该权限，则授权成功
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        // TODO: 通过数据库 + 缓存实现鉴权
        // 1. 从缓存（如 Redis）中读取用户权限列表
        // 2. 如果缓存不存在，则从数据库加载用户权限，并写入缓存

        return Task.CompletedTask;
    }
}
