using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Renova.Core.Common.Extensions;
using Renova.Core.Components.Response;

namespace Renova.Core.Common.AspNetCore.Exceptions;

/// <summary>
///  基于 IExceptionHandler 的全局异常处理 (.NET8 新增)
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IApiResponseProvider _responseProvider;
    private readonly JsonSerializerSettings? _jsonSettings;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        IApiResponseProvider responseProvider,
        IOptions<MvcNewtonsoftJsonOptions>? newtonsoftJsonOptions = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _responseProvider = responseProvider ?? throw new ArgumentNullException(nameof(responseProvider));
        _jsonSettings = newtonsoftJsonOptions?.Value?.SerializerSettings;
    }

    public async ValueTask<bool> TryHandleAsync(
         HttpContext httpContext,
         Exception exception,
         CancellationToken cancellationToken)
    {
        // 如果是 WebSocket 请求，跳过处理
        if (httpContext.IsWebSocketRequest())
        {
            return false;
        }

        // 记录异常详情（包含环境信息）
        var traceId = httpContext.TraceIdentifier;
        var requestPath = httpContext.Request.Path;

        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Environment: {Environment}",
            traceId,
            requestPath,
            _environment.EnvironmentName);


        object? errorResponse = _responseProvider.OnException(httpContext, exception);

        if (errorResponse == null)
        {
            errorResponse = new { Code = StatusCodes.Status500InternalServerError, exception.Message, Data = (object?)null, Timestamp = DateTime.Now };
        }

        // 手动序列化
        string json = JsonConvert.SerializeObject(errorResponse, _jsonSettings);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        await httpContext.Response.WriteAsync(json, cancellationToken);
        return true;
    }

    //public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    //{
    //    // 如果是 WebSocket 请求，跳过处理
    //    if (httpContext.WebSockets.IsWebSocketRequest)
    //    {
    //        return false;
    //    }

    //    var traceId = httpContext.TraceIdentifier;
    //    var path = httpContext.Request.Path;

    //    // 记录异常日志
    //    _logger.LogError(exception, "全局异常捕获，TraceId: {TraceId}", traceId);

    //    var problem = new ProblemDetails
    //    {
    //        Type = "https://httpstatuses.com/500",
    //        Title = "服务器内部错误",
    //        Status = StatusCodes.Status500InternalServerError,
    //        Detail = _env.IsDevelopment() ? exception.ToString() : exception.Message,
    //        Instance = path
    //    };

    //    var response = ApiResponse.Error(code: (int)ApiCode.InternalError, message: exception.Message, data: problem);

    //    var json = JsonSerializer.Serialize(response);

    //    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
    //    httpContext.Response.ContentType = "application/json";

    //    await httpContext.Response.WriteAsJsonAsync(json, cancellationToken);
    //    return true;
    //}
}