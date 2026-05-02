namespace Renova.Rbac.Metadata;

/// <summary>
/// RBAC 内置权限编码。
/// </summary>
public static class RbacPermissionCodes
{
    /// <summary>
    /// 认证相关权限。
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// 查看当前登录用户信息。
        /// </summary>
        public const string Profile = "auth:profile";
    }

    /// <summary>
    /// 用户相关权限。
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// 分页查询用户。
        /// </summary>
        public const string View = "user:view";

        /// <summary>
        /// 创建用户。
        /// </summary>
        public const string Create = "user:create";

        /// <summary>
        /// 更新用户。
        /// </summary>
        public const string Update = "user:update";

        /// <summary>
        /// 删除用户。
        /// </summary>
        public const string Delete = "user:delete";

        /// <summary>
        /// 设置用户状态。
        /// </summary>
        public const string SetStatus = "user:set-status";

        /// <summary>
        /// 分配用户角色。
        /// </summary>
        public const string AssignRole = "user:assign-role";
    }

    /// <summary>
    /// 角色相关权限。
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// 分页查询角色。
        /// </summary>
        public const string View = "role:view";

        /// <summary>
        /// 创建角色。
        /// </summary>
        public const string Create = "role:create";

        /// <summary>
        /// 更新角色。
        /// </summary>
        public const string Update = "role:update";

        /// <summary>
        /// 删除角色。
        /// </summary>
        public const string Delete = "role:delete";

        /// <summary>
        /// 设置角色状态。
        /// </summary>
        public const string SetStatus = "role:set-status";

        /// <summary>
        /// 分配角色权限。
        /// </summary>
        public const string AssignPermission = "role:assign-permission";
    }

    /// <summary>
    /// 权限相关权限。
    /// </summary>
    public static class Permissions
    {
        /// <summary>
        /// 分页查询权限。
        /// </summary>
        public const string View = "permission:view";

        /// <summary>
        /// 创建权限。
        /// </summary>
        public const string Create = "permission:create";

        /// <summary>
        /// 更新权限。
        /// </summary>
        public const string Update = "permission:update";

        /// <summary>
        /// 删除权限。
        /// </summary>
        public const string Delete = "permission:delete";

        /// <summary>
        /// 设置权限状态。
        /// </summary>
        public const string SetStatus = "permission:set-status";
    }
}
