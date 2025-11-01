using Renova.Core.Components.Job;

namespace Renova.Core;

/// <summary>
/// 作业接口
/// </summary>
public interface IJob
{
    /// <summary>
    /// 执行作业
    /// </summary>
    /// <returns></returns>
    Task ExecuteAsync(JobParameters? parameters);
}
