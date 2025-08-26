using Microsoft.AspNetCore.Mvc;
using System;

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
        /// 返回一句话
        /// https://localhost:1031/api/LocalizationTest/Sentence
        /// https://localhost:1031/api/LocalizationTest/Sentence?culture=zh-cn
        /// https://localhost:1031/api/LocalizationTest/Sentence?culture=en-us
        /// https://localhost:1031/api/LocalizationTest/Sentence?culture=ja-jp
        /// </summary>
        [HttpGet("Sentence")]
        public IActionResult Sentence()
        {
            return Ok("时光跌跌撞撞，季节来来往往。".L10N());
        }

        /// <summary>
        /// 返回一个简单的本地化示例
        /// https://localhost:1031/api/LocalizationTest/Hello
        /// https://localhost:1031/api/LocalizationTest/Hello?culture=zh-cn
        /// https://localhost:1031/api/LocalizationTest/Hello?culture=en-us
        /// https://localhost:1031/api/LocalizationTest/Hello?culture=ja-jp
        /// </summary>
        [HttpGet("Hello")]
        public IActionResult Hello()
        {
            return Ok("你好，世界！".L10N());
        }

        /// <summary>
        /// 带参数的本地化示例
        /// https://localhost:1031/api/LocalizationTest/Greet
        /// https://localhost:1031/api/LocalizationTest/Greet?culture=zh-cn
        /// https://localhost:1031/api/LocalizationTest/Greet?culture=en-us
        /// https://localhost:1031/api/LocalizationTest/Greet?culture=ja-jp
        /// </summary>
        [HttpGet("Greet")]
        public IActionResult Greet()
        {
            var dayOfWeekKey = DateTime.Now.DayOfWeek.ToString(); // e.g. "Tuesday"
            var localizedDayOfWeek = dayOfWeekKey.L10N(); // 翻译星期几
            return Ok("你好，{0}，今天是{1}。".L10NFormat("Renova", localizedDayOfWeek));
        }
    }
}
