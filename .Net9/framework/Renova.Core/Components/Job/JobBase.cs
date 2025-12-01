using Microsoft.AspNetCore.Http;
using NewLife.Caching;
using Renova.Core.Apps;
using Renova.Core.Components.Job;
using System.Reflection;
using System.Security.Claims;

namespace Renova.Core;

/// <summary>
/// 作业基类，封装了作业执行的公共逻辑
/// </summary>
public abstract class JobBase : IJob
{
    /// <summary>
    /// 执行作业逻辑（子类实现）
    /// </summary>
    public abstract Task ExecuteAsync(JobParameters? parameters);

    /// <summary>
    /// 运行作业（入口）
    /// </summary>
    public virtual async Task RunAsync(JobConfig jobConfig)
    {
        try
        {
            if (jobConfig == null) throw new ArgumentNullException(nameof(jobConfig));

            var lockkey = GetKey();
            var cache = App.GetRequiredService<ICacheProvider>(App.RootServices).Cache;

            using var jobLock = cache.TryLock(lockkey);
            if (jobLock == null)
            {
                AddLog("当前作业正在执行中,本次作业跳过执行");
                return;
            }

            SetupHttpContext(jobConfig);
            await ExecuteAsync(CreateJobParameters(jobConfig));
        }
        catch (Exception ex)
        {
            AddErrorLog(ex.ToString());
        }
    }

    /// <summary>
    /// 获取作业锁的键名
    /// </summary>
    public virtual string GetKey()
    {
        var type = GetType();
        return $"{type.FullName},{type.Assembly.GetName().Name}";
    }

    /// <summary>
    /// 为后台作业设置 HttpContext
    /// </summary>
    private void SetupHttpContext(JobConfig jobConfig)
    {
        if (App.HttpContext != null) return;

        var accessor = App.GetRequiredService<IHttpContextAccessor>(App.RootServices);
        var context = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                Path = "/background-job",
                QueryString = QueryString.Empty
            }

        };

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "HangfireJob"),
            new Claim(ClaimTypes.NameIdentifier, "hangfire-system"),
            new Claim("tenantId", "tenantId"),
            new Claim("tenantName", "tenantName")
        };

        var identity = new ClaimsIdentity(claims, "Hangfire");
        context.User = new ClaimsPrincipal(identity);
        accessor.HttpContext = context;
    }

    /// <summary>
    /// 作业参数实例初始化
    /// </summary>
    protected virtual JobParameters CreateJobParameters(JobConfig jobConfig)
    {
        var attribute = GetType().GetCustomAttribute<JobAttribute>();
        var parametersType = attribute?.ParametersType ?? typeof(JobParameters);

        if (!typeof(JobParameters).IsAssignableFrom(parametersType))
        {
            throw new InvalidOperationException($"JobAttribute.ParametersType 必须继承自 JobParameters，但实际类型为: {parametersType.FullName}");
        }

        var jobParameter = (JobParameters)Activator.CreateInstance(parametersType)!;
        jobParameter.Initialize(jobConfig.Parameters ?? string.Empty);
        return jobParameter;
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    protected virtual void AddLog(string msg)
    {
        Console.WriteLine(msg);
    }

    /// <summary>
    /// 添加错误日志
    /// </summary>
    protected virtual void AddErrorLog(string msg)
    {
        Console.WriteLine("ERROR: " + msg);
    }
}
