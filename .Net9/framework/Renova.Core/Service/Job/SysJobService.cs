using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renova.Core.Components.Job;
using Simple.DynamicWebApi;
using System.ComponentModel;

namespace Renova.Core.Service.Job;

/// <summary>
/// 系统定时任务服务
/// </summary>
[AllowAnonymous]
public class SysJobService : IDynamicWebApi, ITransientDependency
{
    private readonly IRecurringJobManager _recurringJobs;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SysJobService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SysJobService(IRecurringJobManager recurringJobs, IServiceProvider serviceProvider, ILogger<SysJobService> logger)
    {
        _recurringJobs = recurringJobs;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 添加或更新定时任务
    /// </summary>
    [HttpPost("jobs")]
    [DisplayName("添加或更新定时任务")]
    public void AddOrUpdateJob(JobConfig jobConfig)
    {
        jobConfig.Validate();

        var jobType = GetJobType(jobConfig.JobClass);

        _recurringJobs.AddOrUpdate(
            recurringJobId: jobConfig.RecurringJobId,
            methodCall: () => ((JobBase)ActivatorUtilities.CreateInstance(_serviceProvider, jobType)).RunAsync(jobConfig),
            cronExpression: jobConfig.CronExpression,
            options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
    }

    /// <summary>
    /// 获取作业类型
    /// </summary>
    [DisplayName("获取作业类型")]
    private Type GetJobType(string jobClassName)
    {
        var type = Type.GetType(jobClassName, throwOnError: false, ignoreCase: false);
        if (type == null)
        {
            throw new InvalidOperationException($"无法找到作业类型 '{jobClassName}'。请确保类型名称包含完整命名空间和程序集");
        }

        if (!typeof(JobBase).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"作业类型 '{jobClassName}' 必须继承自 JobBase 基类");
        }

        return type;
    }

    /// <summary>
    /// 立即同步执行 JobDemo 任务（测试在 JobDemo 中是否可以获取 http 上下文--可以）
    /// </summary>
    [DisplayName("立即同步执行 JobDemo 任务")]
    public async Task TriggerJobDemo()
    {
        const string jobClassName = "Renova.Core.Jobs.JobDemo, Renova.Core";

        var jobType = Type.GetType(jobClassName, throwOnError: true);
        if (jobType == null || !typeof(JobBase).IsAssignableFrom(jobType))
        {
            throw new InvalidOperationException($"指定的 Job 类型无效: {jobClassName}");
        }

        var jobConfig = new JobConfig
        {
            JobClass = jobClassName,
            RecurringJobId = "",
            CronExpression = ""
        };

        var jobInstance = (JobBase)ActivatorUtilities.CreateInstance(_serviceProvider, jobType);

        await jobInstance.RunAsync(jobConfig);
    }

    /// <summary>
    /// 触发定时任务立即执行
    /// </summary>
    /// <param name="recurringJobId"></param>
    public void TriggerJob(string recurringJobId)
    {
        _recurringJobs.Trigger(recurringJobId);
    }
}
