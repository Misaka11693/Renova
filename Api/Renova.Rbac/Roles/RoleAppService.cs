namespace Renova.Rbac.Roles;

/// <summary>
/// RBAC 角色动态接口。
/// </summary>
[ApiExplorerSettings(GroupName = "RBAC-角色")]
public class RoleAppService : IDynamicWebApi, ITransientDependency
{
    private readonly RoleDomainService _roleDomainService;

    /// <summary>
    /// 初始化角色应用服务。
    /// </summary>
    public RoleAppService(RoleDomainService roleDomainService)
    {
        _roleDomainService = roleDomainService;
    }

    /// <summary>
    /// 分页获取角色列表。
    /// </summary>
    [DisplayName("分页获取角色")]
    [Permission(RbacPermissionCodes.Roles.View)]
    public Task<SqlSugarPagedList<RoleOutput>> GetPageAsync([FromBody] RolePageInput input)
    {
        return _roleDomainService.GetPageAsync(input);
    }

    /// <summary>
    /// 创建角色。
    /// </summary>
    [DisplayName("创建角色")]
    [Permission(RbacPermissionCodes.Roles.Create)]
    public Task<RoleOutput> CreateAsync([FromBody] CreateRoleInput input)
    {
        return _roleDomainService.CreateAsync(input);
    }

    /// <summary>
    /// 更新角色。
    /// </summary>
    [DisplayName("更新角色")]
    [Permission(RbacPermissionCodes.Roles.Update)]
    public Task<RoleOutput> UpdateAsync([FromBody] UpdateRoleInput input)
    {
        return _roleDomainService.UpdateAsync(input);
    }

    /// <summary>
    /// 删除角色。
    /// </summary>
    [DisplayName("删除角色")]
    [Permission(RbacPermissionCodes.Roles.Delete)]
    public Task<string> DeleteAsync([FromBody] DeleteRoleInput input)
    {
        return _roleDomainService.DeleteAsync(input);
    }

    /// <summary>
    /// 设置角色状态。
    /// </summary>
    [DisplayName("设置角色状态")]
    [Permission(RbacPermissionCodes.Roles.SetStatus)]
    public Task<string> SetStatusAsync([FromBody] SetRoleStatusInput input)
    {
        return _roleDomainService.SetStatusAsync(input);
    }

    /// <summary>
    /// 分配角色权限。
    /// </summary>
    [DisplayName("分配角色权限")]
    [Permission(RbacPermissionCodes.Roles.AssignPermission)]
    public Task<string> AssignPermissionsAsync([FromBody] AssignRolePermissionsInput input)
    {
        return _roleDomainService.AssignPermissionsAsync(input);
    }
}
