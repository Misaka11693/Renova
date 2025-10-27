using SqlSugar;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// SqlSuga数据库连接配置信息
/// </summary>
public class SqlSugarOptions : ConnectionConfig
{
    public static readonly string SectionName = "SqlSugarOptions:ConnectionStrings";

    /// <summary>
    /// 启用控制台打印SQL
    /// </summary>
    public bool EnableConsoleSql { get; set; } = false;
}
