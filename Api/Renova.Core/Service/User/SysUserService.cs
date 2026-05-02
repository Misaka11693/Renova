using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Renova.Core.Components.Security.Authentication.Abstractions;
using Renova.Core.Components.Security.Authentication.Const;
using Renova.Core.Components.Security.Authorization.Attributes;
using Simple.DynamicWebApi;
using System.ComponentModel;
using System.Security.Claims;

namespace Renova.Core.Service;

/// <summary>
/// 系统用户服务
/// </summary>
//[Authorize("s")]
[ApiExplorerSettings(GroupName = "系统用户服务")]
public class SysUserService : IDynamicWebApi, ITransientDependency
{
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SysUserService(SqlSugarRepository<SysUser> sysUserRep, IJwtTokenService jwtTokenService)
    {
        _sysUserRep = sysUserRep;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    //[AllowAnonymous]
    [DisplayName("获取用户分页列表")]
    //[Permission("Get")]
    //[Authorize]
    //[Authorize(AuthenticationSchemes = AuthSchemes.RefreshToken)]
    public virtual async Task<SqlSugarPagedList<SysUser>> GetUserInfo()
    {
        var list = await _sysUserRep.AsQueryable().ToSqlSugarPagedListAsync(2, 1);

        //删除SysUser所有用户，试一下是真删除还是软删除
        //await _sysUserRep.AsDeleteable().Where(u => u.Id > 0).IsLogic()
        //        .ExecuteCommandAsync();

        return list;
    }

    /// <summary>
    /// LoginRequest
    /// </summary>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    public record LoginRequest2(string Username, string Password);

    /// <summary>
    /// 用户登录
    /// </summary>
    [AllowAnonymous]
    public Object Login2([FromBody] LoginRequest2 request)
    {
        //if (request.Username != "admin" || request.Password != "c4ca4238a0b923820dcc509a6f75849b")
        //{
        //    //return Unauthorized("用户名或密码错误");
        //    throw new UserFriendlyException("用户名或密码错误");
        //}

        var user = _sysUserRep.AsQueryable().First(u => u.Account == request.Username && u.Password == request.Password);
        if (user == null)
        {
            throw new Exception("用户名或密码错误");
        }

        // 用户声明
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // 用户ID
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("ua", Apps.App.HttpContext!.Request.Headers.UserAgent.ToString()),
                //new Claim("ua", this.HttpContext.Request.Headers.UserAgent.ToString()),
                //new Claim("Permission", "WeatherForecast:Get")//按钮权限 todo：改为 数据库 + redis缓存
            };

        // 生成 AccessToken 和 RefreshToken
        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(claims);

        return new
        {
            access_token = accessToken,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// 添加用户
    /// </summary>
    [AllowAnonymous]
    [DisplayName("添加用户信息")]
    public virtual async Task<bool> AddUser()
    {
        var user = new SysUser();
        user.GenerateId();
        user.Account = "admin2";
        user.Password = "123456";
        user.NickName = "管理员2";
        user.Status = 1;
        user.CreateTime = DateTime.Now;
        user.CreateUserName = "admin";
        user.CreateUserId = 1;
        _sysUserRep.Insert(user);
        return await Task.FromResult(false);
    }
}

