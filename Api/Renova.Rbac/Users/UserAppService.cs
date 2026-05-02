namespace Renova.Rbac.Users;

/// <summary>
/// RBAC 用户动态接口。
/// </summary>
[ApiExplorerSettings(GroupName = "RBAC-用户")]
public class UserAppService : IDynamicWebApi, ITransientDependency
{
    private readonly UserDomainService _userDomainService;

    /// <summary>
    /// 初始化用户应用服务。
    /// </summary>
    public UserAppService(UserDomainService userDomainService)
    {
        _userDomainService = userDomainService;
    }

    /// <summary>
    /// 分页获取用户列表。
    /// </summary>
    [DisplayName("分页获取用户")]
    [Permission(RbacPermissionCodes.Users.View)]
    public Task<SqlSugarPagedList<UserOutput>> GetPageAsync([FromBody] UserPageInput input)
    {
        return _userDomainService.GetPageAsync(input);
    }

    /// <summary>
    /// 创建用户。
    /// </summary>
    [DisplayName("创建用户")]
    [Permission(RbacPermissionCodes.Users.Create)]
    public Task<UserOutput> CreateAsync([FromBody] CreateUserInput input)
    {
        return _userDomainService.CreateAsync(input);
    }

    /// <summary>
    /// 更新用户。
    /// </summary>
    [DisplayName("更新用户")]
    [Permission(RbacPermissionCodes.Users.Update)]
    public Task<UserOutput> UpdateAsync([FromBody] UpdateUserInput input)
    {
        return _userDomainService.UpdateAsync(input);
    }

    /// <summary>
    /// 删除用户。
    /// </summary>
    [DisplayName("删除用户")]
    [Permission(RbacPermissionCodes.Users.Delete)]
    public Task<string> DeleteAsync([FromBody] DeleteUserInput input)
    {
        return _userDomainService.DeleteAsync(input);
    }

    /// <summary>
    /// 设置用户状态。
    /// </summary>
    [DisplayName("设置用户状态")]
    [Permission(RbacPermissionCodes.Users.SetStatus)]
    public Task<string> SetStatusAsync([FromBody] SetUserStatusInput input)
    {
        return _userDomainService.SetStatusAsync(input);
    }

    /// <summary>
    /// 分配用户角色。
    /// </summary>
    [DisplayName("分配用户角色")]
    [Permission(RbacPermissionCodes.Users.AssignRole)]
    public Task<string> AssignRolesAsync([FromBody] AssignUserRolesInput input)
    {
        return _userDomainService.AssignRolesAsync(input);
    }
}
