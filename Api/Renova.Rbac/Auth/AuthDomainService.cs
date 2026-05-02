// ReSharper disable All
namespace Renova.Rbac.Auth;

/// <summary>
/// 认证领域服务，负责登录、刷新令牌和 RBAC 初始化。
/// </summary>
public class AuthDomainService : ITransientDependency
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly CurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDomainService _userDomainService;
    private readonly RoleDomainService _roleDomainService;
    private readonly PermissionDomainService _permissionDomainService;

    /// <summary>
    /// 初始化认证领域服务。
    /// </summary>
    public AuthDomainService(
        IJwtTokenService jwtTokenService,
        CurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor,
        UserDomainService userDomainService,
        RoleDomainService roleDomainService,
        PermissionDomainService permissionDomainService)
    {
        _jwtTokenService = jwtTokenService;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _userDomainService = userDomainService;
        _roleDomainService = roleDomainService;
        _permissionDomainService = permissionDomainService;
    }

    /// <summary>
    /// 校验账号密码并返回令牌结果。
    /// </summary>
    public async Task<TokenOutput> LoginAsync(LoginInput input)
    {
        var user = await _userDomainService.GetByAccountAsync(input.UserName);
        if (user == null || !PasswordHasher.Verify(input.Password, user.Password))
        {
            throw new UserFriendlyException("用户名或密码错误。");
        }

        if (user.Status != (int)CommonStatus.Enabled)
        {
            throw new UserFriendlyException("当前用户已被禁用。");
        }

        return await BuildTokenOutputAsync(user);
    }

    /// <summary>
    /// 重新签发访问令牌。
    /// </summary>
    public async Task<TokenOutput> RefreshAsync()
    {
        var userId = _currentUser.GetRequired().UserId;
        var user = await _userDomainService.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UserFriendlyException("用户不存在。");
        }

        return await BuildTokenOutputAsync(user);
    }

    /// <summary>
    /// 获取当前登录用户的基础信息、角色和权限。
    /// </summary>
    public async Task<CurrentUserOutput> GetCurrentUserAsync()
    {
        var userId = _currentUser.GetRequired().UserId;
        var user = await _userDomainService.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UserFriendlyException("用户不存在。");
        }

        return new CurrentUserOutput
        {
            UserId = user.Id,
            Account = user.Account,
            NickName = user.NickName,
            Roles = await _userDomainService.GetRoleCodesAsync(userId),
            Permissions = await _userDomainService.GetPermissionCodesAsync(userId)
        };
    }

    /// <summary>
    /// 初始化最小可用的 RBAC 数据。
    /// </summary>
    public async Task<string> BootstrapAsync()
    {
        var adminUser = await _userDomainService.EnsureSeedUserAsync(
            RbacDefaults.AdminAccount,
            RbacDefaults.AdminPassword,
            "系统管理员");

        var adminRole = await _roleDomainService.EnsureSeedRoleAsync(
            RbacDefaults.AdminRoleCode,
            "超级管理员",
            1,
            "系统初始化角色");

        var permissions = new List<SysPermission>();
        foreach (var definition in RbacPermissionDefinitions.All)
        {
            var permission = await _permissionDomainService.EnsureSeedPermissionAsync(definition);
            permissions.Add(permission);
        }

        await _userDomainService.EnsureUserRoleAsync(adminUser.Id, adminRole.Id);

        foreach (var permission in permissions)
        {
            await _roleDomainService.EnsureRolePermissionAsync(adminRole.Id, permission.Id);
        }

        return $"RBAC 初始化完成。默认账号: {RbacDefaults.AdminAccount}，默认密码: {RbacDefaults.AdminPassword}";
    }

    /// <summary>
    /// 为指定用户构造令牌输出结果。
    /// </summary>
    private async Task<TokenOutput> BuildTokenOutputAsync(SysUser user)
    {
        var roleCodes = await _userDomainService.GetRoleCodesAsync(user.Id);
        var permissionCodes = await _userDomainService.GetPermissionCodesAsync(user.Id);
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimConst.UserId, user.Id.ToString()),
            new(ClaimTypes.Name, user.Account),
            new(ClaimConst.Account, user.Account),
            new(ClaimConst.NickName, user.NickName ?? string.Empty),
            new("ua", userAgent)
        };

        claims.AddRange(roleCodes.Select(x => new Claim(ClaimTypes.Role, x)));
        claims.AddRange(permissionCodes.Select(x => new Claim("Permission", x)));

        return new TokenOutput
        {
            UserId = user.Id,
            UserName = user.Account,
            NickName = user.NickName,
            Roles = roleCodes,
            Permissions = permissionCodes,
            AccessToken = _jwtTokenService.GenerateAccessToken(claims),
            RefreshToken = _jwtTokenService.GenerateRefreshToken(claims)
        };
    }
}
