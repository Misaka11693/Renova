using Renova.Core.Common;
using SqlSugar;
using System.Runtime;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// 数据库连接选项
/// </summary>
public class DbConnectionOptions : IConfigSectionProvider
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public static string SectionName => "DbConnectionOptions";

    /// <summary>
    /// 启用控制台打印SQL
    /// </summary>
    public bool EnableConsoleSql { get; set; } = false;

    /// <summary>
    /// 数据库集合
    /// </summary>
    public List<DbConnectionConfig> ConnectionConfigs { get; set; } = new List<DbConnectionConfig>();
}

/// <summary>
/// 数据库连接配置
/// </summary>
public sealed class DbConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public DbSettings DbSettings { get; set; } = new DbSettings();

    /// <summary>
    /// 表配置
    /// </summary>
    public TableSettings TableSettings { get; set; } = new TableSettings();

    /// <summary>
    /// 种子配置
    /// </summary>
    public SeedSettings SeedSettings { get; set; } = new SeedSettings();

    /// <summary>
    /// 隔离方式
    /// </summary>
    public TenantTypeEnum TenantType { get; set; } = TenantTypeEnum.Id;
}

/// <summary>
/// 数据库配置
/// </summary>
public sealed class DbSettings
{
    /// <summary>
    /// 启用库表初始化
    /// </summary>
    public bool EnableInitDb { get; set; }

    /// <summary>
    /// 启用库表差异日志
    /// </summary>
    public bool EnableDiffLog { get; set; }

    /// <summary>
    /// 启用驼峰转下划线
    /// </summary>
    public bool EnableUnderLine { get; set; }

    /// <summary>
    /// 启用数据库连接串加密策略
    /// </summary>
    public bool EnableConnStringEncrypt { get; set; }
}

/// <summary>
/// 表配置
/// </summary>
public sealed class TableSettings
{
    /// <summary>
    /// 启用表初始化
    /// </summary>
    public bool EnableInitTable { get; set; }

    /// <summary>
    /// 启用表增量更新
    /// </summary>
    public bool EnableIncreTable { get; set; }
}

/// <summary>
/// 种子配置
/// </summary>
public sealed class SeedSettings
{
    /// <summary>
    /// 启用种子初始化
    /// </summary>
    public bool EnableInitSeed { get; set; }

    /// <summary>
    /// 启用种子增量更新
    /// </summary>
    public bool EnableIncreSeed { get; set; }
}