using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Renova.Core;

/// <summary>
/// 工作单元特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class UnitOfWorkAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// 过滤器执行顺序：靠后执行，在其他业务过滤器之后开启事务
    /// </summary>
    public int Order => 9999;

    /// <summary>
    /// 拦截 Web API Action 执行
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
        if (method == null)
        {
            await next();
            return;
        }

        var services = context.HttpContext.RequestServices;
        var logger = services.GetRequiredService<ILogger<UnitOfWorkAttribute>>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var attribute = this;

        unitOfWork.BeginTransaction(context, attribute);

        ActionExecutedContext? executedContext = null;

        try
        {
            executedContext = await next();

            if (executedContext.Exception == null || executedContext.ExceptionHandled)
            {
                unitOfWork.CommitTransaction(executedContext, attribute);
            }
            else
            {
                unitOfWork.RollbackTransaction(executedContext, attribute);
            }
        }
        catch (Exception)
        {
            unitOfWork.RollbackTransaction(context, attribute);
            throw;
        }
        finally
        {
            unitOfWork.OnCompleted(context, executedContext);
        }
    }
}
