using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Renova.Core.Common.Attributes;
using Renova.Core.Common.Extensions;

namespace Renova.Core.Components.Response;

/// <summary>
/// API 统一响应格式过滤器
/// 将指定类型的返回值统一包装成 ApiResponse 格式
/// </summary>
public class ApiResponseFilter : IAsyncResultFilter
{
    /// <summary>
    /// API 统一响应提供器
    /// </summary>
    private readonly IApiResponseProvider _responseProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiResponseFilter(IApiResponseProvider provider)
    {
        _responseProvider = provider ?? throw new ArgumentNullException(nameof(provider));
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

    /// <summary>
    ///  执行结果过滤器
    /// </summary>
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
        context.Result = (JsonResult)_responseProvider.OnSucceeded(context, data);

        await next();
    }

    /// <summary>
    /// 判断当前结果是否需要包装
    /// </summary>
    private bool ShouldWrap(ResultExecutingContext context)
    {
        // 1.跳过 WebSocket 请求
        if (context.HttpContext.IsWebSocketRequest())
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
            var code = statusCodeResult.StatusCode ?? StatusCodes.Status200OK;
            return code is >= 200 and < 300;
        }

        return true;
    }

    /// <summary>
    /// 从 ActionResult 中提取状态码和数据
    /// </summary>
    private static (int StatusCode, object? Data) ExtractResponseInfo(ResultExecutingContext context)
    {
        int statusCode = StatusCodes.Status200OK; // 默认 200

        if (context.Result is IStatusCodeActionResult statusCodeResult)
        {
            statusCode = statusCodeResult.StatusCode ?? StatusCodes.Status200OK;
        }

        return context.Result switch
        {
            ObjectResult o => (o.StatusCode ?? statusCode, o.Value),
            JsonResult j => (j.StatusCode ?? statusCode, j.Value),
            ContentResult c => (c.StatusCode ?? statusCode, c.Content),
            _ => (statusCode, null) // 理论上不会触发
        };
    }
}
