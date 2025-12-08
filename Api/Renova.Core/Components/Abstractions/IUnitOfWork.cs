using Microsoft.AspNetCore.Mvc.Filters;

namespace Renova.Core;

/// <summary>
/// 工作单元接口
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 开启工作单元
    /// </summary>
    /// <param name="context">Action 执行前的上下文</param>
    /// <param name="unitOfWork">触发该工作单元的特性实例</param>
    void BeginTransaction(FilterContext context, UnitOfWorkAttribute unitOfWork);

    /// <summary>
    /// 提交工作单元
    /// </summary>
    /// <param name="resultContext">Action 执行后的上下文</param>
    /// <param name="unitOfWork">触发该工作单元的特性实例</param>
    void CommitTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork);

    /// <summary>
    /// 回滚工作单元
    /// </summary>
    /// <param name="resultContext">包含异常信息的执行后上下文</param>
    /// <param name="unitOfWork">触发该工作单元的特性实例</param>
    void RollbackTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork);

    /// <summary>
    /// 工作单元执行完毕（无论成功失败）
    /// </summary>
    /// <param name="context">Action 执行前的上下文</param>
    /// <param name="resultContext">Action 执行后的上下文，异常路径中可能为 null</param>
    void OnCompleted(FilterContext context, FilterContext? resultContext);
}