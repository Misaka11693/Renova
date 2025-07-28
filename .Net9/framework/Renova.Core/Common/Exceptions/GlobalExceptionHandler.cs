using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Renova.Core;

/// <summary>
///  基于 IExceptionHandler 的全局异常处理 (.NET8 新增)
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path;

        // 记录异常日志
        _logger.LogError(exception, "全局异常捕获，TraceId: {TraceId}", traceId);

        var problem = new ProblemDetails
        {
            Type = "https://httpstatuses.com/500",
            Title = "服务器内部错误",
            Status = StatusCodes.Status500InternalServerError,
            Detail = _env.IsDevelopment() ? exception.ToString() : exception.Message,
            Instance = path
        };

        var response = ApiResponse.Error(code: (int)ApiCode.InternalError, message: exception.Message, data: problem);

        var json = JsonSerializer.Serialize(response);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(json, cancellationToken);
        return true;
    }
}
