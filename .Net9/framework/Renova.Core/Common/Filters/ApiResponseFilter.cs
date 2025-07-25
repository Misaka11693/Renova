using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Renova.Core;

/// <summary>
///  Api 统一响应格式过滤器
/// </summary>
public class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (ShouldSkipWrapper(context))
        {
            await next();
            return;
        }

        // 包装结果
        if (context.Result is ObjectResult objectResult)
        {
            var wrapped = new ApiResponse<object>
            {
                Code = (int)ApiCode.Success,
                Message = "成功",
                Data = objectResult.Value
            };

            context.Result = new ObjectResult(wrapped)
            {
                StatusCode = objectResult.StatusCode
            };
        }
        else if (context.Result is JsonResult jsonResult)
        {
            var wrapped = new ApiResponse<object>
            {
                Code = (int)ApiCode.Success,
                Message = "成功",
                Data = jsonResult.Value
            };

            context.Result = new JsonResult(wrapped)
            {
                StatusCode = jsonResult.StatusCode
            };
        }
        else if (context.Result is EmptyResult)
        {
            var wrapped = new ApiResponse<object>
            {
                Code = (int)ApiCode.Success,
                Message = "成功",
                Data = null
            };

            context.Result = new ObjectResult(wrapped);
        }

        await next();
    }


    private bool ShouldSkipWrapper(ResultExecutingContext context)
    {
        var result = context.Result;

        // 排除特殊类型
        if (result is FileResult || result is RedirectResult || result is ChallengeResult || result is SignInResult || result is SignOutResult)
        {
            return true;
        }

        // 已是 ApiResponse<T> 不再包装
        if (result is ObjectResult objResult && IsApiResponse(objResult.Value))
        {
            return true;
        }

        // 标记 [SkipWrapAttribute]  不再包装
        if (context.ActionDescriptor.EndpointMetadata.Any(m => m.GetType().Name == "SkipWrapAttribute"))
        {
            return true;
        }

        return false;
    }

    private bool IsApiResponse(object? value)
    {
        if (value == null) return false;

        var type = value.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }
}
