using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Renova.Core.Components.Response;

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
    /// 构建错误响应
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    object OnException(HttpContext httpContext, Exception exception);

    /// <summary>
    /// 拦截返回状态码
    /// </summary>
    /// <returns></returns>
    Task OnResponseStatusCodes(HttpContext context, int statusCode);
}
