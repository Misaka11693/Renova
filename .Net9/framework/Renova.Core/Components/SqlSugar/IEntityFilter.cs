using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// 软删除标记接口
/// </summary>
public interface ISoftDeleteFilter
{
    /// <summary>
    /// 软删除标记
    /// </summary>
    bool IsDeleted { get; set; }
}

/// <summary>
/// 租户Id接口接口
/// </summary>
internal interface ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    long? TenantId { get; set; }
}

/// <summary>
/// 部门Id接口过滤器
/// </summary>
internal interface IOrgIdFilter
{
    /// <summary>
    /// 创建者部门Id
    /// </summary>
    long? CreateOrgId { get; set; }
}