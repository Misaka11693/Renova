using Microsoft.AspNetCore.Mvc;
using Renova.Core;
using System.Text;

namespace Renova.WebApi.Controllers
{
    /// <summary>
    /// API响应测试控制器。
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "API响应测试")]
    public class ApiResponseTestController : ControllerBase
    {
        /// <summary>
        /// 正常返回对象（应被包装）
        /// </summary>
        [HttpGet("object-result")]
        public IActionResult GetObjectResult()
        {
            return Ok(new { Name = "John", Age = 30 });
        }

        /// <summary>
        /// 返回JSON结果（应被包装）
        /// </summary>
        [HttpGet("json-result")]
        public IActionResult GetJsonResult()
        {
            return new JsonResult(new { Id = 1, Value = "Test" }) { StatusCode = 200 };
        }

        /// <summary>
        /// 返回空结果（应被包装）
        /// </summary>
        [HttpGet("empty-result")]
        public IActionResult GetEmptyResult()
        {
            return new EmptyResult();
        }

        /// <summary>
        /// 返回文件结果（应跳过包装）
        /// </summary>
        [HttpGet("file-result")]
        public IActionResult GetFileResult()
        {
            var bytes = Encoding.UTF8.GetBytes("Test file content");
            return File(bytes, "text/plain", "test.txt");
        }

        /// <summary>
        /// 直接返回ApiResponse（应跳过包装）
        /// </summary>
        [HttpGet("pre-wrapped")]
        public IActionResult GetPreWrapped()
        {
            var response = ApiResponse<object>.Success(new { Data = "Pre-wrapped" });
            return Ok(response);
        }

        /// <summary>
        /// 标记SkipWrap特性的方法（应跳过包装）
        /// </summary>
        [HttpGet("skip-wrap")]
        [SkipWrap] // 需要先定义SkipWrap特性
        public IActionResult GetWithSkipWrap()
        {
            return Ok(new { Message = "Skipped wrapping" });
        }

        /// <summary>
        /// 模拟业务失败响应
        /// </summary>
        [HttpGet("business-fail")]
        public IActionResult BusinessFailure()
        {
            // 实际项目中应返回适当的状态码
            return BadRequest(ApiResponse<object>.Fail("业务操作失败", ApiCode.Conflict));
        }

        /// <summary>
        /// 返回404结果（应被包装）
        /// </summary>
        [HttpGet("not-found")]
        public IActionResult GetNotFound()
        {
            return NotFound(new { Error = "Item not found" });
        }
    }
}


// 1.正常对象结果 (GET /api/test/object-result)
// 2.JSON结果 (GET /api/test/json-result)
// 3.空结果 (GET /api/test/empty-result)
// 4.文件下载结果 (GET /api/test/file-result)
// 5.预包装的API响应 (GET /api/test/pre-wrapped)
// 6.跳过包装的结果 (GET /api/test/skip-wrap)
// 7.业务失败示例 (GET /api/test/business-fail)
// 8.返回404错误 (GET /api/test/not-found)