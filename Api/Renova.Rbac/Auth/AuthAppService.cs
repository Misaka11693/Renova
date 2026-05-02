namespace Renova.Rbac.Auth;

/// <summary>
/// RBAC 认证动态接口。
/// </summary>
[ApiExplorerSettings(GroupName = "RBAC-认证")]
public class AuthAppService : IDynamicWebApi, ITransientDependency
{
    private readonly AuthDomainService _authDomainService;

    /// <summary>
    /// 初始化认证应用服务。
    /// </summary>
    public AuthAppService(AuthDomainService authDomainService)
    {
        _authDomainService = authDomainService;
    }

    /// <summary>
    /// 初始化内置管理员、角色和权限数据。
    /// </summary>
    [AllowAnonymous]
    [DisplayName("RBAC 初始化")]
    public Task<string> BootstrapAsync()
    {
        return _authDomainService.BootstrapAsync();
    }

    /// <summary>
    /// 根据账号密码登录并签发令牌。
    /// </summary>
    [AllowAnonymous]
    [DisplayName("用户登录")]
    public Task<TokenOutput> LoginAsync([FromBody] LoginInput input)
    {
        return _authDomainService.LoginAsync(input);
    }

    /// <summary>
    /// 使用刷新令牌重新签发访问令牌。
    /// </summary>
    [Authorize(AuthenticationSchemes = AuthSchemes.RefreshToken)]
    [DisplayName("刷新令牌")]
    public Task<TokenOutput> RefreshAsync()
    {
        return _authDomainService.RefreshAsync();
    }

    /// <summary>
    /// 获取当前登录用户信息。
    /// </summary>
    [DisplayName("获取当前用户")]
    [Permission(RbacPermissionCodes.Auth.Profile)]
    public Task<CurrentUserOutput> GetCurrentUserAsync()
    {
        return _authDomainService.GetCurrentUserAsync();
    }
}
