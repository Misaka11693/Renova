using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Renova.Core.Response;
using System.Diagnostics;
using System.Text;

namespace Renova.Core.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IApiResponseProvider _responseProvider;
    private readonly JsonSerializerSettings? _jsonSettings;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment,
        IApiResponseProvider responseProvider,
        IOptions<MvcNewtonsoftJsonOptions>? newtonsoftJsonOptions = null)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _responseProvider = responseProvider;
        _jsonSettings = newtonsoftJsonOptions?.Value?.SerializerSettings;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            var requestPath = context.Request.Path;

            _logger.LogError(
                ex,
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Environment: {Environment}",
                traceId,
                requestPath,
                _environment.EnvironmentName);

            object? errorResponse = _responseProvider.OnException(context, ex);
            if (errorResponse == null)
            {
                errorResponse = new
                {
                    Code = StatusCodes.Status500InternalServerError,
                    ex.Message,
                    Data = (object?)null,
                    Timestamp = DateTime.Now
                };
            }

            string json = JsonConvert.SerializeObject(errorResponse, _jsonSettings ?? new JsonSerializerSettings());

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            await context.Response.WriteAsync(json, Encoding.UTF8, context.RequestAborted);
        }
    }
}