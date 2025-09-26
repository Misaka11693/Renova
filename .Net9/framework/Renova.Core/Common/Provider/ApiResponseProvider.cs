using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Renova.Core;

public class ApiResponseProvider : IApiResponseProvider
{
    public IActionResult OnError(ActionExecutedContext context, object? data, int statusCode, string? message = null)
    {
        throw new NotImplementedException();
    }

    public Task OnResponseStatusCodes(HttpContext context, int statusCode)
    {
        throw new NotImplementedException();
    }

    public IActionResult OnSucceeded(ResultExecutingContext context, object? data)
    {
        return new JsonResult(ApiResponse.Success(data));
    }
}
