using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Renova.Core.Components.Response;

/// <summary>
/// API 统一响应提供器
/// </summary>
public class ApiResponseProvider : IApiResponseProvider
{
    private readonly IHostEnvironment _environment;
    private readonly JsonSerializerSettings? _jsonSerializerSettings;

    /// <summary>
    /// 构造函数    
    /// </summary>
    public ApiResponseProvider(
        IHostEnvironment environment,
        IOptions<MvcNewtonsoftJsonOptions>? newtonsoftJsonOptions = null)
    {
        _environment = environment;
        _jsonSerializerSettings = newtonsoftJsonOptions?.Value?.SerializerSettings;
    }

    /// <summary>
    /// 构建成功响应
    /// </summary>
    public IActionResult OnSucceeded(ResultExecutingContext context, object? data)
    {
        var response = RestfulResult(statusCode: StatusCodes.Status200OK, message: "请求成功", data: data);
        //return new JsonResult(response, _jsonSerializerSettings); //无需使用_jsonSerializerSettings配置序列化格式
        return new JsonResult(response);
    }

    /// <summary>
    /// 构建错误响应
    /// </summary>
    public object OnException(HttpContext httpContext, Exception exception)
    {
        var response = RestfulResult(statusCode: StatusCodes.Status500InternalServerError, errors: exception.Message);
        //return new JsonResult(response, _jsonSerializerSettings); //无需使用_jsonSerializerSettings配置序列化格式
        //return new JsonResult(response);
        return response;
    }

    /// <summary>
    /// 拦截返回状态码
    /// </summary>
    public async Task OnResponseStatusCodes(HttpContext context, int statusCode)
    {
        if (context.Response.HasStarted)
            return;

        object? response = null;

        switch (statusCode)
        {
            case StatusCodes.Status401Unauthorized:
                response = RestfulResult(statusCode: statusCode, errors: "登录已过期，请重新登录");
                break;

            case StatusCodes.Status403Forbidden:
                response = RestfulResult(statusCode: statusCode, errors: "禁止访问，没有权限", data: context.Request.Path);
                break;

            default:
                return; // 不处理的状态码直接跳过
        }

        string json = JsonConvert.SerializeObject(response, _jsonSerializerSettings); // 需要使用_jsonSerializerSettings配置序列化格式

        //context.Response.Clear();
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(json, Encoding.UTF8, context.RequestAborted);
    }

    /// <summary>
    /// RestfulResult 风格响应结果
    /// </summary>
    private ApiResponse RestfulResult(int statusCode, string? message = null, object? data = null, object? errors = null)
    {
        return new ApiResponse
        {
            Code = statusCode,
            Message = statusCode == StatusCodes.Status200OK ? "请求成功" : errors,
            Data = data
        };
    }
}
