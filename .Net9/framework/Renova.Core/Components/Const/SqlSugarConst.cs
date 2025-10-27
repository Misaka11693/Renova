namespace Renova.Core.Components.Const;

/// <summary>
/// SqlSugar相关常量
/// </summary>
public class SqlSugarConst
{
    /// <summary>
    /// 默认主数据库标识（默认租户）
    /// </summary>
    public const string MainConfigId = "10001";

    /// <summary>
    /// 默认日志数据库标识
    /// </summary>
    public const string LogConfigId = "10002";

    /// <summary>
    /// 默认表主键
    /// </summary>
    public const string PrimaryKey = "Id";

    /// <summary>
    /// 默认租户Id
    /// </summary>
    public const long DefaultTenantId = 10001;
}
