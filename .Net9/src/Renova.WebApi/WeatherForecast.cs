namespace Renova
{
    /// <summary>
    /// ����Ԥ��ģ��
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// ����
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// ���϶��¶�
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// �����¶�
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// ����ժҪ
        /// </summary>
        public string? Summary { get; set; }
    }
}
