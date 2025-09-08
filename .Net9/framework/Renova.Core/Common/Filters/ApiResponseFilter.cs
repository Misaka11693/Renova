using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Renova.Core;

/// <summary>
/// API 统一响应格式过滤器
/// 将指定类型的返回值统一包装成 ApiResponse 格式
/// </summary>
public class ApiResponseFilter : IAsyncResultFilter
{
    /// <summary>
    /// 可被包装的 ActionResult 类型
    /// </summary>
    private static readonly Type[] WrappableResultTypes =
    {
        typeof(ObjectResult),     // Ok(), BadRequest(), return object
        typeof(JsonResult),       // return Json(...)
        typeof(ContentResult),    // return "string"
        typeof(StatusCodeResult), // return StatusCode(200)（无数据）
        typeof(EmptyResult)       // return NoContent()
    };

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // 跳过不需要包装的结果
        if (!ShouldWrap(context))
        {
            await next();
            return;
        }

        // 提取状态码和返回值
        var (statusCode, value) = ExtractResponseInfo(context);

        // 成功返回 2xx，失败统一提示“操作失败”
        var wrapped = statusCode is >= 200 and < 300
            ? ApiResponse.Success(value, statusCode)
            : ApiResponse.Error(value, statusCode);

        context.Result = new ObjectResult(wrapped) { StatusCode = statusCode };

        await next();
    }

    /// <summary>
    /// 判断当前结果是否需要包装
    /// </summary>
    private bool ShouldWrap(ResultExecutingContext context)
    {
        var resultType = context.Result.GetType();

        // 跳过标记了 [SkipWrap] 的接口
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipWrapAttribute>().Any())
            return false;

        // 已经是 ApiResponse 类型的结果不再包装
        if (context.Result is ObjectResult { Value: ApiResponse })
            return false;

        // 只包装允许的类型
        return WrappableResultTypes.Any(t => t.IsAssignableFrom(resultType));
    }

    /// <summary>
    /// 从 ActionResult 中提取状态码和数据
    /// </summary>
    private static (int StatusCode, object? Data) ExtractResponseInfo(ResultExecutingContext context)
    {
        var defaultCode = context.HttpContext.Response.StatusCode;

        return context.Result switch
        {
            ObjectResult o => (o.StatusCode ?? defaultCode, o.Value),
            JsonResult j => (j.StatusCode ?? defaultCode, j.Value),
            ContentResult c => (c.StatusCode ?? defaultCode, c.Content),
            StatusCodeResult s => (s.StatusCode, null), // StatusCodeResult 无数据
            EmptyResult => (defaultCode, null),
            _ => (defaultCode, null)
        };
    }
}
