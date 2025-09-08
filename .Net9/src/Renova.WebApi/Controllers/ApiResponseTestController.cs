using Microsoft.AspNetCore.Mvc;
using Renova.Core;
using System.Text;

namespace Renova.WebApi.Controllers
{
    /// <summary>
    /// API响应测试控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "API统一响应包装测试")]
    public class ApiResponseTestController : ControllerBase
    {
        /// <summary>
        /// 正常对象结果（自动包装为ApiResponse）
        /// </summary>
        [HttpGet("standard-object")]
        public IActionResult GetStandardObject()
        {
            return Ok(new { Id = 1, Name = "Test Object" });
        }

        /// <summary>
        /// 自定义状态码结果（自动包装并保留状态码）
        /// </summary>
        [HttpGet("custom-status")]
        public IActionResult GetWithCustomStatus()
        {
            //return StatusCode(StatusCodes.Status202Accepted, new { Message = "Processing" });
            return StatusCode(StatusCodes.Status202Accepted, "Processing");

        }

        /// <summary>
        /// 空结果（包装为无数据的成功响应）
        /// </summary>
        [HttpGet("empty-response")]
        public IActionResult GetEmptyResponse()
        {
            return new EmptyResult();
        }

        /// <summary>
        /// 错误结果（自动包装为错误格式）
        /// </summary>
        [HttpGet("error-response")]
        public IActionResult GetErrorResponse()
        {
            return NotFound(new { ErrorDetails = "Resource not found" });
        }

        /// <summary>
        /// 文件下载（跳过包装）
        /// </summary>
        [HttpGet("download-file")]
        public IActionResult DownloadFile()
        {
            var fileContent = Encoding.UTF8.GetBytes("Test file content");
            return File(fileContent, "text/plain", "test.txt");
        }

        /// <summary>
        /// 预包装的ApiResponse（跳过二次包装）
        /// </summary>
        [HttpGet("pre-wrapped")]
        public IActionResult GetPreWrappedResponse()
        {
            var response = ApiResponse.Success(new { Data = "Already wrapped" });
            return Ok(response);
        }

        /// <summary>
        /// 跳过包装的端点（直接返回原始格式）
        /// </summary>
        [HttpGet("skip-wrap")]
        [SkipWrap]
        public IActionResult GetUnwrappedResponse()
        {
            return Ok(new { RawData = "This won't be wrapped" });
        }

        /// <summary>
        /// 业务验证失败示例（标准错误格式）
        /// </summary>
        [HttpGet("validation-fail")]
        public IActionResult SimulateValidationFail()
        {
            return BadRequest(new { Field = "Name", Error = "Required" });
        }

        /// <summary>
        /// 服务器错误（自动捕获500状态码）
        /// </summary>
        [HttpGet("server-error")]
        public IActionResult SimulateServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal error details");
        }

        /// <summary>
        /// 重定向测试（跳过包装）
        /// </summary>
        [HttpGet("redirect")]
        public IActionResult RedirectTest()
        {
            return Redirect("https://example.com");
        }

        /// <summary>
        /// 内容结果测试（自动包装）
        /// </summary>
        [HttpGet("content-result")]
        public ContentResult GetContentResult()
        {
            return Content("<html>Test</html>", "text/html");
        }

        /// <summary>
        /// 混合错误码业务响应
        /// </summary>
        [HttpGet("business-error")]
        public IActionResult BusinessError()
        {
            // 业务逻辑错误使用标准错误码
            return StatusCode(StatusCodes.Status409Conflict,
                new { TransactionId = 123, Reason = "Duplicate request" });
        }

        /// <summary>
        /// 无内容成功（204状态码特殊处理）
        /// </summary>
        [HttpGet("no-content")]
        public IActionResult NoContentResult()
        {
            return NoContent(); // 204状态码
        }

        /// <summary>
        /// 返回纯字符串内容（测试过滤器对字符串的包装）
        /// </summary>
        [HttpGet("string-result")]
        [SkipWrap]
        public IActionResult GetStringResult()
        {
            return Ok("This is a raw string response");
        }

        /// <summary>
        /// 直接返回字符串（非IActionResult，测试ContentResult自动包装）
        /// </summary>
        [HttpGet("direct-string")]
        public string GetDirectString()
        {
            return "Direct string without IActionResult";
        }

                /// <summary>
        /// 模拟 401 未认证
        /// </summary>
        [HttpGet("unauthorized")]
        public IActionResult SimulateUnauthorized()
        {
            // 返回 401 状态码
            //return Unauthorized(new { Message = "You are not authenticated." });
            return Unauthorized("You are not authenticated.");
        }

        /// <summary>
        /// 模拟 403 禁止访问
        /// </summary>
        [HttpGet("forbidden")]
        public IActionResult SimulateForbidden()
        {
            // 返回 403 状态码
            return Forbid(); // 也可以用 StatusCode(StatusCodes.Status403Forbidden)
        }
    }
}