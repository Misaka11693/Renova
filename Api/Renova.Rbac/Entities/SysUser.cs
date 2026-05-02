namespace Renova.Rbac.Entities;

/// <summary>
/// 系统用户实体。
/// </summary>
[SysTable]
[SugarTable("sys_user", "系统用户表")]
public class SysUser : EntityBase
{
    /// <summary>
    /// 登录账号。
    /// </summary>
    [SugarColumn(ColumnDescription = "登录账号", Length = 50, IsNullable = false)]
    [Required(ErrorMessage = "登录账号不能为空。")]
    [MaxLength(50, ErrorMessage = "登录账号长度不能超过 50 个字符。")]
    public virtual string Account { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码。
    /// </summary>
    [SugarColumn(ColumnDescription = "登录密码", Length = 100, IsNullable = false)]
    [Required(ErrorMessage = "登录密码不能为空。")]
    [MaxLength(100, ErrorMessage = "登录密码长度不能超过 100 个字符。")]
    public virtual string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称。
    /// </summary>
    [SugarColumn(ColumnDescription = "用户昵称", Length = 50, IsNullable = true)]
    [MaxLength(50, ErrorMessage = "用户昵称长度不能超过 50 个字符。")]
    public virtual string? NickName { get; set; }

    /// <summary>
    /// 用户状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "用户状态", IsNullable = false)]
    public virtual int Status { get; set; } = (int)CommonStatus.Enabled;
}
