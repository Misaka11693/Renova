using NewLife.Caching;
using Renova.Core.Apps;
using Renova.Core.Components.Job;
using System.Reflection;

namespace Renova.Core;

/// <summary>
/// 作业基类，封装了作业执行的公共逻辑
/// </summary>
public abstract class JobBase : IJob
{
    /// <summary>
    /// 执行作业逻辑（子类必须实现）
    /// </summary>
    /// <param name="parameters">作业参数</param>
    public abstract Task ExecuteAsync(JobParameters? parameters);

    /// <summary>
    /// 运行作业（入口）
    /// </summary>
    /// <param name="jobConfig">作业配置</param>
    public virtual async Task RunAsync(JobConfig jobConfig)
    {
        try
        {
            if (jobConfig == null)
                throw new ArgumentNullException(nameof(jobConfig));

            // 获取锁，防止并发执行
            string lockkey = GetKey(GetType());
            var cacheProvider = App.GetRequiredService<ICacheProvider>(App.RootServices);

            using var jobLock = cacheProvider.Cache.TryLock(lockkey);
            if (jobLock == null)
            {
                AddLog("当前作业正在执行中,本次作业跳过执行");
                return;
            }

            //测试锁定状态
            //for (int i = 0; i < 9999; i++)
            //{
            //    await Task.Delay(10_000);
            //}

            // 作业执行前调用
            await OnExecuting(jobConfig);

            var attribute = this.GetType().GetCustomAttribute<JobAttribute>();
            var parametersType = attribute?.ParametersType ?? typeof(JobParameters);

            if (!typeof(JobParameters).IsAssignableFrom(parametersType))
            {
                throw new InvalidOperationException($"JobAttribute.ParametersType 必须继承自 JobParameters，但实际类型为: {parametersType.FullName}");
            }

            var jobParameter = (JobParameters)Activator.CreateInstance(parametersType)!;
            jobParameter.Initialize(jobConfig.Parameters ?? string.Empty);

            // 执行核心逻辑
            await ExecuteAsync(jobParameter);
        }
        catch (Exception ex)
        {
            AddErroLog(ex.ToString());
        }
    }

    /// <summary>
    /// 作业执行前调用
    /// </summary>
    protected virtual Task OnExecuting(JobConfig jobConfig)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取作业锁的键名
    /// </summary>
    protected virtual string GetKey(Type jobClass)
    {
        return $"{jobClass.FullName},{jobClass.Assembly.GetName().Name}";
    }


    /// <summary>
    /// 添加日志，在调度任务执行中需要记录执行日志
    /// </summary>
    protected virtual void AddLog(string msg)
    {
        Console.WriteLine(msg);
    }

    /// <summary>
    /// 添加异常日志，在调度任务执行中需要记录执行日志
    /// </summary>
    protected virtual void AddErroLog(string msg)
    {
        Console.WriteLine("ERROR: " + msg);
    }
}
