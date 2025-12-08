using SqlSugar;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// 主键实体基类
/// </summary>
public abstract class PrimaryKeyEntity
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键Id", IsPrimaryKey = true, IsIdentity = false)]
    public virtual long Id { get; set; }

    /// <summary>
    ///  生成主键Id
    /// </summary>
    public virtual void GenerateId()
    {
        this.Id = Yitter.IdGenerator.YitIdHelper.NextId();
    }

    /// <summary>
    /// 是否有主键Id
    /// </summary>
    /// <returns></returns>
    public virtual bool HasId()
    {
        return this.Id > 0;
    }
}

/// <summary>
/// 租户实体基类Id
/// </summary>
public abstract class EntityTenantId : PrimaryKeyEntity, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}

/// <summary>
/// 业务数据实体基类（数据权限）
/// </summary>
public abstract class EntityBaseData : EntityBase, IOrgIdFilter
{
    /// <summary>
    /// 创建者部门Id
    /// </summary>
    [OwnerOrg]
    [SugarColumn(ColumnDescription = "创建者部门Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? CreateOrgId { get; set; }

    /// <summary>
    /// 创建者部门
    /// </summary>
    //[Newtonsoft.Json.JsonIgnore]
    //[System.Text.Json.Serialization.JsonIgnore]
    //[Navigate(NavigateType.OneToOne, nameof(CreateOrgId))]
    //public virtual SysOrg CreateOrg { get; set; }

    /// <summary>
    /// 创建者部门名称
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者部门名称", Length = 64, IsOnlyIgnoreUpdate = true)]
    public virtual string? CreateOrgName { get; set; }
}

/// <summary>
/// 框架实体基类
/// </summary>
public abstract class EntityBase : PrimaryKeyEntity, ISoftDeleteFilter
{
    /// <summary>
    /// 创建人Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建人Id", IsOnlyIgnoreUpdate = true, IsNullable = false)]
    public required virtual long CreateUserId { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [SugarColumn(ColumnDescription = "创建人姓名", IsOnlyIgnoreUpdate = true, IsNullable = false)]
    public required virtual string CreateUserName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true, IsNullable = false)]
    public required DateTime CreateTime { get; set; }

    /// <summary>
    /// 修改人Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改人Id", IsOnlyIgnoreInsert = true, IsNullable = true)]
    public virtual long? UpdateUserId { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [SugarColumn(ColumnDescription = "修改人姓名", IsOnlyIgnoreInsert = true, IsNullable = true)]
    public virtual string? UpdateUserName { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [SugarColumn(ColumnDescription = "修改时间", IsOnlyIgnoreInsert = true, IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 软删除标记
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除标记", IsNullable = false)]
    public bool IsDeleted { get; set; } = false;
}
