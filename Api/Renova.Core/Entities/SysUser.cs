using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Renova.Core;

/// <summary>
/// 系统用户表
/// </summary>
[SysTable]
[SugarTable(null, "系统用户表")]
//[SugarTable("Sys_User", "系统用户表")]
public class SysUser : EntityBase
{
    /// <summary>
    /// 登录账号
    /// </summary>
    [SugarColumn(ColumnDescription = "登录账号", Length = 50, IsNullable = false)]
    [Required(ErrorMessage = "登录账号不能为空")]
    [MaxLength(50, ErrorMessage = "账号长度不能超过50")]
    public virtual string Account { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码
    /// </summary>
    [SugarColumn(ColumnDescription = "登录密码", Length = 100, IsNullable = false)]
    [Required(ErrorMessage = "密码不能为空")]
    [MaxLength(100, ErrorMessage = "密码长度不能超过100")]
    public virtual string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "用户昵称", Length = 50, IsNullable = true)]
    [MaxLength(50, ErrorMessage = "昵称长度不能超过50")]
    public virtual string? NickName { get; set; }

    /// <summary>
    /// 用户状态 (0正常 1停用)
    /// </summary>
    [SugarColumn(ColumnDescription = "用户状态", IsNullable = false)]
    public virtual int Status { get; set; } = 0;
}