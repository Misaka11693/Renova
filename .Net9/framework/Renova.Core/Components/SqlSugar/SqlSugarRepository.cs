using Renova.Core.Apps;
using Renova.Core.Components.Const;
using SqlSugar;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// SqlSugar仓储实现
/// </summary>
public class SqlSugarRepository<T> : SimpleClient<T>, ISqlSugarRepository<T> where T : class, new()
{
    public ITenant iTenant { get; }

    public SqlSugarRepository(ISqlSugarClient context)
    {
        iTenant = context as ITenant ?? throw new ArgumentException(nameof(context));

        base.Context = ResolveConnectionScope();
    }

    private SqlSugarScopeProvider ResolveConnectionScope()
    {
        var type = typeof(T);
        var tenantId = App.User?.FindFirst(ClaimConst.TenantId)?.Value;

        // 1. 如果标记了 [Tenant] 特性 → 使用租户专属库（分库）
        if (type.IsDefined(typeof(TenantAttribute), false))
        {
            return iTenant.GetConnectionScopeWithAttr<T>();
        }

        // 2. 如果标记了 [LogTable] 特性 → 使用日志库
        if (type.IsDefined(typeof(LogTableAttribute), false))
        {
            return iTenant.IsAnyConnection(SqlSugarConst.LogConfigId)
                ? iTenant.GetConnectionScope(SqlSugarConst.LogConfigId)
                : throw new Exception("日志数据库连接未配置，请检查配置文件！");
        }

        // 3. 如果标记了 [SysTable] 特性 → 使用主库（平台数据）
        if (type.IsDefined(typeof(SysTableAttribute), false))
        {
            return iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
        }

        // 4.若未贴任何表特性或当前未登录或是默认租户Id，则返回默认库连接
        if (string.IsNullOrWhiteSpace(tenantId) || tenantId == SqlSugarConst.MainConfigId)
        {
            return iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
        }

        // 5.根据租户Id切换库连接 为空则返回默认库连接
        var sqlSugarScopeProviderTenant = GetTenantDbConnectionScope(long.Parse(tenantId));
        return sqlSugarScopeProviderTenant ?? iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
    }

    private SqlSugarScopeProvider GetTenantDbConnectionScope(long tenantId)
    {
        // 若已存在租户库连接，则直接返回
        if (iTenant.IsAnyConnection(tenantId.ToString()))
        {
            return iTenant.GetConnectionScope(tenantId.ToString());
        }

        lock (iTenant)
        {
            //从租户信息表获取租户数据库连接信息：todo
            throw new InvalidOperationException($"租户数据库连接未配置，租户ID: {tenantId}");
        }
    }
}
