using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Renova.Core.Components.Security.Contexts;

/// <summary>
/// 当前用户服务。
/// </summary>
public class CurrentUser : IScopedDependency
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private CurrentUserInfo? _cachedUser;
    private bool _isResolved;

    /// <summary>
    /// 初始化当前用户服务。
    /// </summary>
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 当前请求是否已登录。
    /// </summary>
    public bool IsAuthenticated => GetOrNull() != null;

    /// <summary>
    /// 当前用户 Id，不存在时返回空。
    /// </summary>
    public long? UserId => GetOrNull()?.UserId;

    /// <summary>
    /// 当前登录账号，不存在时返回空。
    /// </summary>
    public string? Account => GetOrNull()?.Account;

    /// <summary>
    /// 当前用户名，不存在时返回空。
    /// </summary>
    public string? UserName => GetOrNull()?.UserName;

    /// <summary>
    /// 当前用户昵称，不存在时返回空。
    /// </summary>
    public string? NickName => GetOrNull()?.NickName;

    /// <summary>
    /// 获取当前用户，不存在时返回空。
    /// </summary>
    public CurrentUserInfo? GetOrNull()
    {
        if (_isResolved)
        {
            return _cachedUser;
        }

        _cachedUser = ResolveCurrentUser();
        _isResolved = true;
        return _cachedUser;
    }

    /// <summary>
    /// 获取当前用户，不存在时抛出异常。
    /// </summary>
    public CurrentUserInfo GetRequired()
    {
        return GetOrNull() ?? throw new UserFriendlyException("无法解析当前用户。");
    }

    /// <summary>
    /// 解析当前请求中的用户信息。
    /// </summary>
    private CurrentUserInfo? ResolveCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var value = user.FindFirst(ClaimConst.UserId)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(value, out var userId))
        {
            return null;
        }

        return new CurrentUserInfo
        {
            UserId = userId,
            Account = user.FindFirst(ClaimConst.Account)?.Value,
            UserName = user.Identity?.Name,
            NickName = user.FindFirst(ClaimConst.NickName)?.Value
        };
    }
}
