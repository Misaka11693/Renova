using Microsoft.AspNetCore.Mvc.Filters;
using SqlSugar;

namespace Renova.Core;

/// <summary>
/// 基于 SqlSugar 的工作单元实现
/// </summary>
public sealed class SqlSugarUnitOfWork : IUnitOfWork
{
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SqlSugarUnitOfWork(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 开启 SqlSugar 工作单元
    /// </summary>
    public void BeginTransaction(FilterContext context, UnitOfWorkAttribute unitOfWork)
    {
        _sqlSugarClient.AsTenant().BeginTran();
    }

    /// <summary>
    /// 提交当前工作单元
    /// </summary>
    public void CommitTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
    {
        _sqlSugarClient.AsTenant().CommitTran();
    }

    /// <summary>
    /// 回滚当前工作单元
    /// </summary>
    public void RollbackTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
    {
        _sqlSugarClient.AsTenant().RollbackTran();
    }

    /// <summary>
    /// 释放 SqlSugar 客户端资源。
    /// </summary>
    public void OnCompleted(FilterContext context, FilterContext? resultContext)
    {
        _sqlSugarClient.Dispose();
    }
}