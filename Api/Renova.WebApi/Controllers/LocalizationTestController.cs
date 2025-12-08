using Microsoft.AspNetCore.Mvc;
using Renova.Core.Components.Localization;

namespace Renova.WebApi.Controllers
{
    /// <summary>
    /// 本地化测试控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "本地化测试")]
    public class LocalizationTestController : ControllerBase
    {
        /// <summary>
        /// 返回一句本地化的句子。
        /// </summary>
        /// <remarks>
        /// 示例调用：
        /// <list type="bullet">
        ///   <item><description>默认语言：https://localhost:1031/api/LocalizationTest/Sentence</description></item>
        ///   <item><description>中文：https://localhost:1031/api/LocalizationTest/Sentence?culture=zh-cn</description></item>
        ///   <item><description>英文：https://localhost:1031/api/LocalizationTest/Sentence?culture=en-us</description></item>
        ///   <item><description>日文：https://localhost:1031/api/LocalizationTest/Sentence?culture=ja-jp</description></item>
        /// </list>
        /// </remarks>
        [HttpGet("Sentence")]
        public IActionResult Sentence()
        {
            return Ok("时光跌跌撞撞，季节来来往往。".L10N());
        }

        /// <summary>
        /// 返回一句包含参数的本地化问候语。
        /// </summary>
        /// <remarks>
        /// 示例调用：
        /// <list type="bullet">
        ///   <item><description>默认语言：https://localhost:1031/api/LocalizationTest/Greet</description></item>
        ///   <item><description>中文：https://localhost:1031/api/LocalizationTest/Greet?culture=zh-cn</description></item>
        ///   <item><description>英文：https://localhost:1031/api/LocalizationTest/Greet?culture=en-us</description></item>
        ///   <item><description>日文：https://localhost:1031/api/LocalizationTest/Greet?culture=ja-jp</description></item>
        /// </list>
        /// </remarks>
        [HttpGet("Greet")]
        public IActionResult Greet()
        {
            var dayOfWeekKey = DateTime.Now.DayOfWeek.ToString(); 
            var localizedDayOfWeek = dayOfWeekKey.L10N();
            return Ok("你好，{0}，今天是{1}。".L10NFormat("Renova", localizedDayOfWeek));
        }
    }
}
