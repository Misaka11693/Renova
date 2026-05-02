namespace Renova.Rbac.Users.Models;

/// <summary>
/// 分配用户角色请求参数。
/// </summary>
public class AssignUserRolesInput
{
    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 角色 Id 集合。
    /// </summary>
    public List<long> RoleIds { get; set; } = new();
}
