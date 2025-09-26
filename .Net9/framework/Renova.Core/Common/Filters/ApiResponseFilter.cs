using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Renova.Core;

/// <summary>
/// API 统一响应格式过滤器
/// 将指定类型的返回值统一包装成 ApiResponse 格式
/// </summary>
public class ApiResponseFilter : IAsyncResultFilter
{
    /// <summary>
    /// API 统一响应提供器
    /// </summary>
    private readonly IApiResponseProvider _provider;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiResponseFilter(IApiResponseProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// 可被包装的 ActionResult 类型
    /// </summary>
    private static readonly Type[] WrappableResultTypes =
    {
        typeof(ObjectResult),     // Ok(), BadRequest(), return object
        typeof(JsonResult),       // return Json(...)
        typeof(ContentResult),    // return "string"
        //typeof(StatusCodeResult), // return StatusCode(200)（无数据）2025-9-26 移除对 StatusCodeResult 的包装
        //typeof(EmptyResult)       // return NoContent() 2025-9-26 移除对 EmptyResult 的包装
    };

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // 跳过不需要包装的结果 or 响应已开始写入
        if (!ShouldWrap(context) || context.HttpContext.Response.HasStarted)
        {
            await next();
            return;
        }

        // 提取状态码和返回值
        var (statusCode, data) = ExtractResponseInfo(context);
        context.Result = _provider.OnSucceeded(context, data);

        await next();


        // 成功返回 2xx，失败统一提示“操作失败”
        //var wrapped = statusCode is >= 200 and < 300
        //    ? ApiResponse.Success(value, statusCode)
        //    : ApiResponse.Error(value, statusCode);

        //context.Result = new ObjectResult(wrapped) { StatusCode = statusCode };

        //await next();
    }

    /// <summary>
    /// 判断当前结果是否需要包装
    /// </summary>
    private bool ShouldWrap(ResultExecutingContext context)
    {
        // 1.跳过 WebSocket 请求
        if (context.HttpContext.WebSockets.IsWebSocketRequest)
            return false;

        // 2.跳过标记了 [SkipWrap] 的接口
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipWrapAttribute>().Any())
            return false;

        // 3.已经是 ApiResponse 类型的结果不再包装
        if (context.Result is ObjectResult { Value: ApiResponse })
            return false;

        // 4.只包装允许的类型
        var resultType = context.Result.GetType();
        if (!WrappableResultTypes.Any(t => t.IsAssignableFrom(resultType)))
            return false;

        // 5.包装 2xx 成功状态码，其他状态码不包装
        if (context.Result is IStatusCodeActionResult statusCodeResult)
        {
            var code = statusCodeResult.StatusCode;
            return code.HasValue && code.Value is >= 200 and < 300;
        }

        return true;
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
            _ => (defaultCode, null) // 理论上不会触发
        };

        //return context.Result switch
        //{
        //    ObjectResult o => (o.StatusCode ?? defaultCode, o.Value),
        //    JsonResult j => (j.StatusCode ?? defaultCode, j.Value),
        //    ContentResult c => (c.StatusCode ?? defaultCode, c.Content),
        //    StatusCodeResult s => (s.StatusCode, null), // StatusCodeResult 无数据
        //    EmptyResult => (defaultCode, null),
        //    _ => (defaultCode, null)
        //};
    }
}
