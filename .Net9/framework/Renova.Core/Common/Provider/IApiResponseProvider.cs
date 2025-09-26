using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Renova.Core;

/// <summary>
/// API 统一响应提供器
/// </summary>
public interface IApiResponseProvider
{
    /// <summary>
    /// 构造成功响应
    /// </summary>
    IActionResult OnSucceeded(ResultExecutingContext context, object? data);

    /// <summary>
    /// 构造错误响应
    /// </summary>
    IActionResult OnError(ActionExecutedContext context, object? data, int statusCode, string? message = null);

    /// <summary>
    /// 拦截返回状态码
    /// </summary>
    /// <returns></returns>
    Task OnResponseStatusCodes(HttpContext context, int statusCode);
}
