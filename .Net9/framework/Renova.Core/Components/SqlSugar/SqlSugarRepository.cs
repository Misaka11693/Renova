using Renova.Core.Apps;
using Renova.Core.Components.Const;
using SqlSugar;
using System.Collections.Concurrent;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// 基于 SqlSugar 的仓储实现，支持多租户。
/// 注意：该仓储应注册为 Scoped 或 Transient，不可单例（因依赖当前用户上下文）。
/// </summary>
public class SqlSugarRepository<T> : SimpleClient<T>, ISqlSugarRepository<T>
    where T : class, new()
{
    /// <summary>
    /// 当前租户上下文
    /// </summary>
    public ITenant iTenant { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">SqlSugar 客户端实例</param>
    public SqlSugarRepository(ISqlSugarClient context)
    {
        iTenant = context as ITenant
                  ?? throw new ArgumentException($"注入的上下文必须实现 {nameof(ITenant)} 接口。", nameof(context));

        base.Context = ResolveConnectionScope();
    }

    /// <summary>
    /// 根据实体类型特性与当前用户上下文，解析应使用的数据库连接
    /// 优先级：[Tenant] > [LogTable] > [SysTable] > 当前租户 > 默认主库
    /// </summary>
    /// <returns>对应数据库的 SqlSugar 连接作用域</returns>
    private SqlSugarScopeProvider ResolveConnectionScope()
    {
        var entityType = typeof(T);
        var tenantIdClaim = App.User?.FindFirst(ClaimConst.TenantId)?.Value;

        // 1. [Tenant] 特性：租户专属业务库
        if (entityType.IsDefined(typeof(TenantAttribute), false))
        {
            return iTenant.GetConnectionScopeWithAttr<T>();
        }

        // 2. [LogTable] 特性：统一日志库
        if (entityType.IsDefined(typeof(LogTableAttribute), false))
        {
            if (iTenant.IsAnyConnection(SqlSugarConst.LogConfigId))
                return iTenant.GetConnectionScope(SqlSugarConst.LogConfigId);
            else
                return iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
        }

        // 3. [SysTable] 特性：平台系统库
        if (entityType.IsDefined(typeof(SysTableAttribute), false))
        {
            return iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
        }

        // 4. 未登录 或 租户ID为主库ID → 使用主库
        if (string.IsNullOrWhiteSpace(tenantIdClaim) || tenantIdClaim == SqlSugarConst.MainConfigId)
        {
            return iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
        }

        // 5. 普通租户：尝试解析租户ID并获取对应数据库连接
        if (long.TryParse(tenantIdClaim, out var tenantId))
        {
            var tenantScope = GetTenantDbConnectionScope(tenantId);
            return tenantScope ?? iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
        }
        else
        {
            // 租户ID非数字（如GUID），可扩展支持，此处暂降级到主库或抛异常
            // 根据业务需求决定：可改为 throw 或 fallback
            //return iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
            throw new InvalidOperationException($"无法解析租户ID: {tenantIdClaim}，请确保租户ID为数字型。");
        }
    }

    /// <summary>
    /// 获取指定租户的数据库连接作用域
    /// </summary>
    /// <param name="tenantId">租户ID（数字型）</param>
    /// <returns>租户对应的连接作用域，若无法创建则返回 null</returns>
    private SqlSugarScopeProvider GetTenantDbConnectionScope(long tenantId)
    {
        var tenantKey = tenantId.ToString();

        // 已存在连接，直接返回
        if (iTenant.IsAnyConnection(tenantKey))
        {
            return iTenant.GetConnectionScope(tenantKey);
        }

        lock (iTenant)
        {
            // 双重检查：防止多个线程同时进入后重复初始化
            if (iTenant.IsAnyConnection(tenantKey))
            {
                return iTenant.GetConnectionScope(tenantKey);
            }

            // TODO: 此处应根据 tenantId 从配置中心或数据库加载连接字符串
            // 示例：
            // var connStr = LoadTenantConnectionString(tenantId);
            // if (string.IsNullOrEmpty(connStr))
            //     return null; // 或抛异常

            // iTenant.AddConnection(tenantKey, connStr); // 假设 iTenant 支持动态注册

            // 当前未实现动态注册，抛出明确异常便于排查
            throw new InvalidOperationException(
                $"租户数据库连接未配置，租户ID: {tenantId}。" +
                "请确保在 iTenant 中已注册该租户连接，或实现动态加载逻辑。");
        }
    }
}