using System.Text.Json;

namespace Renova.Core.Components.Job
{
    /// <summary>
    /// 定时任务配置
    /// </summary>
    public class JobConfig
    {
        /// <summary>
        /// 定时任务ID
        /// </summary>
        public required string RecurringJobId { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public required string CronExpression { get; set; }

        /// <summary>
        /// 定时任务执行类（含完整命名空间，如 "Renova.Core.Jobs.JobDemo, Renova.Core"）
        /// </summary>
        public required string JobClass { get; set; }

        /// <summary>
        /// 定时任务参数（JSON 格式字符串）
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// 验证配置的有效性。
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(RecurringJobId))
                throw new ArgumentException("定时任务ID不能为空.", nameof(RecurringJobId));

            if (string.IsNullOrWhiteSpace(CronExpression))
                throw new ArgumentException("Cron表达式不能为空.", nameof(CronExpression));

            if (string.IsNullOrWhiteSpace(JobClass))
                throw new ArgumentException("任务类不能为空.", nameof(JobClass));

            if (!string.IsNullOrEmpty(Parameters))
            {
                try
                {
                    using var doc = JsonDocument.Parse(Parameters);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException("任务参数不是有效的JSON格式.", nameof(Parameters), ex);
                }
            }
        }
    }
}
