using Microsoft.AspNetCore.Http;
using Renova.Core.Common.Extensions;

namespace Renova.Core.Components.Response;

/// <summary>
/// 统一 API 响应状态码中间件
/// </summary>
public class ApiResponseStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiResponseProvider _responseProvider;


    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiResponseStatusMiddleware(RequestDelegate next, IApiResponseProvider provider)
    {
        _next = next;
        _responseProvider = provider ?? throw new ArgumentNullException(nameof(provider));

    }

    /// <summary>
    /// 中间件执行入口
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        var statusCode = context.Response.StatusCode;


        if (context.IsWebSocketRequest())
        {
            return;
        }

        if (context.Response.HasStarted)
        {
            return;
        }

        await _responseProvider.OnResponseStatusCodes(context, statusCode);
    }
}
