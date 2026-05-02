namespace Renova.Rbac.Metadata;

/// <summary>
/// RBAC 模块内置权限定义。
/// </summary>
public static class RbacPermissionDefinitions
{
    /// <summary>
    /// 所有内置权限。
    /// </summary>
    public static IReadOnlyList<RbacPermissionDefinition> All { get; } =
    [
        new()
        {
            Code = RbacPermissionCodes.Auth.Profile,
            Name = "查看当前用户",
            Path = "/api/services/AuthAppService/GetCurrentUser",
            HttpMethod = "GET",
            Sort = 10
        },
        new()
        {
            Code = RbacPermissionCodes.Users.View,
            Name = "分页获取用户",
            Path = "/api/services/UserAppService/GetPage",
            HttpMethod = "POST",
            Sort = 20
        },
        new()
        {
            Code = RbacPermissionCodes.Users.Create,
            Name = "创建用户",
            Path = "/api/services/UserAppService/Create",
            HttpMethod = "POST",
            Sort = 21
        },
        new()
        {
            Code = RbacPermissionCodes.Users.Update,
            Name = "更新用户",
            Path = "/api/services/UserAppService/Update",
            HttpMethod = "POST",
            Sort = 22
        },
        new()
        {
            Code = RbacPermissionCodes.Users.Delete,
            Name = "删除用户",
            Path = "/api/services/UserAppService/Delete",
            HttpMethod = "POST",
            Sort = 23
        },
        new()
        {
            Code = RbacPermissionCodes.Users.SetStatus,
            Name = "设置用户状态",
            Path = "/api/services/UserAppService/SetStatus",
            HttpMethod = "POST",
            Sort = 24
        },
        new()
        {
            Code = RbacPermissionCodes.Users.AssignRole,
            Name = "分配用户角色",
            Path = "/api/services/UserAppService/AssignRoles",
            HttpMethod = "POST",
            Sort = 25
        },
        new()
        {
            Code = RbacPermissionCodes.Roles.View,
            Name = "分页获取角色",
            Path = "/api/services/RoleAppService/GetPage",
            HttpMethod = "POST",
            Sort = 30
        },
        new()
        {
            Code = RbacPermissionCodes.Roles.Create,
            Name = "创建角色",
            Path = "/api/services/RoleAppService/Create",
            HttpMethod = "POST",
            Sort = 31
        },
        new()
        {
            Code = RbacPermissionCodes.Roles.Update,
            Name = "更新角色",
            Path = "/api/services/RoleAppService/Update",
            HttpMethod = "POST",
            Sort = 32
        },
        new()
        {
            Code = RbacPermissionCodes.Roles.Delete,
            Name = "删除角色",
            Path = "/api/services/RoleAppService/Delete",
            HttpMethod = "POST",
            Sort = 33
        },
        new()
        {
            Code = RbacPermissionCodes.Roles.SetStatus,
            Name = "设置角色状态",
            Path = "/api/services/RoleAppService/SetStatus",
            HttpMethod = "POST",
            Sort = 34
        },
        new()
        {
            Code = RbacPermissionCodes.Roles.AssignPermission,
            Name = "分配角色权限",
            Path = "/api/services/RoleAppService/AssignPermissions",
            HttpMethod = "POST",
            Sort = 35
        },
        new()
        {
            Code = RbacPermissionCodes.Permissions.View,
            Name = "分页获取权限",
            Path = "/api/services/PermissionAppService/GetPage",
            HttpMethod = "POST",
            Sort = 40
        },
        new()
        {
            Code = RbacPermissionCodes.Permissions.Create,
            Name = "创建权限",
            Path = "/api/services/PermissionAppService/Create",
            HttpMethod = "POST",
            Sort = 41
        },
        new()
        {
            Code = RbacPermissionCodes.Permissions.Update,
            Name = "更新权限",
            Path = "/api/services/PermissionAppService/Update",
            HttpMethod = "POST",
            Sort = 42
        },
        new()
        {
            Code = RbacPermissionCodes.Permissions.Delete,
            Name = "删除权限",
            Path = "/api/services/PermissionAppService/Delete",
            HttpMethod = "POST",
            Sort = 43
        },
        new()
        {
            Code = RbacPermissionCodes.Permissions.SetStatus,
            Name = "设置权限状态",
            Path = "/api/services/PermissionAppService/SetStatus",
            HttpMethod = "POST",
            Sort = 44
        }
    ];
}
