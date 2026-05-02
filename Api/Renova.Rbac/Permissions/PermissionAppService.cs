namespace Renova.Rbac.Permissions;

/// <summary>
/// RBAC 权限动态接口。
/// </summary>
[ApiExplorerSettings(GroupName = "RBAC-权限")]
public class PermissionAppService : IDynamicWebApi, ITransientDependency
{
    private readonly PermissionDomainService _permissionDomainService;

    /// <summary>
    /// 初始化权限应用服务。
    /// </summary>
    public PermissionAppService(PermissionDomainService permissionDomainService)
    {
        _permissionDomainService = permissionDomainService;
    }

    /// <summary>
    /// 分页获取权限列表。
    /// </summary>
    [DisplayName("分页获取权限")]
    [HttpPost("page")]
    [Permission(RbacPermissionCodes.Permissions.View)]
    public Task<SqlSugarPagedList<PermissionOutput>> GetPageAsync([FromBody] PermissionPageInput input)
    {
        return _permissionDomainService.GetPageAsync(input);
    }

    /// <summary>
    /// 创建权限。
    /// </summary>
    [DisplayName("创建权限")]
    [Permission(RbacPermissionCodes.Permissions.Create)]
    public Task<PermissionOutput> CreateAsync([FromBody] CreatePermissionInput input)
    {
        return _permissionDomainService.CreateAsync(input);
    }

    /// <summary>
    /// 更新权限。
    /// </summary>
    [DisplayName("更新权限")]
    [Permission(RbacPermissionCodes.Permissions.Update)]
    public Task<PermissionOutput> UpdateAsync([FromBody] UpdatePermissionInput input)
    {
        return _permissionDomainService.UpdateAsync(input);
    }

    /// <summary>
    /// 删除权限。
    /// </summary>
    [DisplayName("删除权限")]
    [Permission(RbacPermissionCodes.Permissions.Delete)]
    public Task<string> DeleteAsync([FromBody] DeletePermissionInput input)
    {
        return _permissionDomainService.DeleteAsync(input);
    }

    /// <summary>
    /// 设置权限状态。
    /// </summary>
    [DisplayName("设置权限状态")]
    [Permission(RbacPermissionCodes.Permissions.SetStatus)]
    public Task<string> SetStatusAsync([FromBody] SetPermissionStatusInput input)
    {
        return _permissionDomainService.SetStatusAsync(input);
    }
}
