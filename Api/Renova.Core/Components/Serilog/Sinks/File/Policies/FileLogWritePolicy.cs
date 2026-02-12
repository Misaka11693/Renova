using Renova.Core.Components.Serilog.Abstractions;
using Serilog.Events;

namespace Renova.Core.Components.Serilog.Sinks.File.Policies
{
    /// <summary>
    /// 文件日志写入策略
    /// </summary>
    public class FileLogWritePolicy : IFileLogWritePolicy
    {
        /// <summary>
        /// 是否应写入日志
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool ShouldWrite(LogEvent e)
            => true;
    }
}
