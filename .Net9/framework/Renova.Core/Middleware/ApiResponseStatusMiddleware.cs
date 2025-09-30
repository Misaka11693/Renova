using Microsoft.AspNetCore.Http;
using Renova.Core.Response;

namespace Renova.Core.Middleware;

/// <summary>
/// 统一 API 响应状态码中间件
/// </summary>
public class ApiResponseStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiResponseProvider _responseProvider;


    public ApiResponseStatusMiddleware(RequestDelegate next, IApiResponseProvider provider)
    {
        _next = next;
        _responseProvider = provider ?? throw new ArgumentNullException(nameof(provider));

    }

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
