using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Core;
using Renova.WebApi.Models;
using Serilog.Context;
using System.ComponentModel;

namespace Renova.Controllers
{
    /// <summary>
    /// 天气预报控制器
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "天气预报")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取天气预报信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        //[Authorize(Policy = "WeatherForecast:Get")]
        [HttpGet(Name = "GetWeatherForecast")]
        [DisplayName("访问接口")]
        public IEnumerable<WeatherForecast> GetData()
        {
            using (LogContext.PushProperty("UserId", 123))
            using (LogContext.PushProperty("OrderId", "ORD-20260118"))
            {
                Log.Information("创建订单");
            }
            //throw new UserFriendlyException("测试全局异常处理");
            Log.Error("访问接口日志 {Time}", DateTime.Now);
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
