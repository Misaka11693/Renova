using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Core;
using Renova.Core.Components.Job;
using Renova.Core.Components.Security.Authentication.Abstractions;
using Renova.Core.Components.Security.Authentication.Const;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Renova.WebApi.Controllers;

/// <summary>
/// AuthController
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "认证授权")]

public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AuthController(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    //[SkipWrap]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request.Username != "admin" || request.Password != "c4ca4238a0b923820dcc509a6f75849b")
        {
            //return Unauthorized("用户名或密码错误");
            throw new UserFriendlyException("用户名或密码错误");
        }

        // 用户声明
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // 用户ID
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("ua", this.HttpContext.Request.Headers.UserAgent.ToString()),
                //new Claim("Permission", "WeatherForecast:Get")//按钮权限 todo：改为 数据库 + redis缓存
            };

        // 生成 AccessToken 和 RefreshToken
        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(claims);

        return Ok(new
        {
            access_token = accessToken,
            RefreshToken = refreshToken
        });
    }

    /// <summary>
    /// 令牌刷新
    /// </summary>
    /// <returns></returns>
    [HttpPost("refresh")]
    [Authorize(AuthenticationSchemes = AuthSchemes.RefreshToken)]
    public IActionResult Refresh()
    {
        // 从 RefreshToken 的 Claims 中获取用户信息
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // 生成新的 AccessToken
        var claims = new List<Claim>
        {
             new Claim(ClaimTypes.NameIdentifier, userId),
             new Claim("ua", this.HttpContext.Request.Headers.UserAgent.ToString()),
        };

        // 生成 AccessToken 和 RefreshToken
        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(claims);

        return Ok(new
        {
            access_token = accessToken,
            RefreshToken = refreshToken
        });
    }

    /// <summary>
    /// LoginRequest
    /// </summary>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    public record LoginRequest(string Username, string Password);

    /// <summary>
    /// RefreshRequest
    /// </summary>
    /// <param name="RefreshToken"></param>
    public record RefreshRequest(string RefreshToken);
}
