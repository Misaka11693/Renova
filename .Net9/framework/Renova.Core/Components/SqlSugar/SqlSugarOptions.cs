using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// SqlSuga数据库连接配置信息
/// </summary>
public class SqlSugarOptions : ConnectionConfig
{
    public static readonly string SectionName = "SqlSugarOptions:ConnectionStrings";
}
