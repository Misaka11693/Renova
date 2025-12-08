namespace Renova.WebApi.Models
{
    /// <summary>
    /// 天气预报模型
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// 摄氏度温度
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// 华氏温度
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// 天气摘要
        /// </summary>
        public string? Summary { get; set; }
    }
}
