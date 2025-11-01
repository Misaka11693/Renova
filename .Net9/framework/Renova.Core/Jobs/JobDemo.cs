using Renova.Core.Apps;
using Renova.Core.Components.Job;

namespace Renova.Core.Jobs;

/// <summary>
/// 定时任务测试
/// </summary>
[Job("定时任务测试")]
public class JobDemo : JobBase
{
    private readonly IMessageService _messageService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public JobDemo(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>
    /// 任务执行
    /// </summary>

    public override Task ExecuteAsync(JobParameters? parameters)
    {
        //测试缓存自动续租
        //for (int i = 0; i < 9999; i++)
        //{
        //    Console.WriteLine("你好");
        //    await Task.Delay(1_000);
        //}

        //控制台输出App.HttpContext 是否为空,用于校验Hangfire作业中HttpContext是否可用
        var httpContext = App.HttpContext;
        Console.WriteLine($"HttpContext 是否为空: {(httpContext == null ? "是" : "否")}");
        Console.WriteLine(_messageService.GetMessage());
        return Task.CompletedTask;
    }
}

/// <summary>
/// 服务接口
/// </summary>

public interface IMessageService
{
    /// <summary>
    /// 获取信息
    /// </summary>
    string GetMessage();
}

/// <summary>
/// 服务实现
/// </summary>
public class MessageService : IMessageService, ITransientDependency
{
    private readonly string _instanceId = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 获取当前服务实例的标识信息
    /// </summary>
    /// <returns>
    /// 返回包含实例 ID 的字符串。  
    /// - 若为 Singleton：所有调用返回相同 ID  
    /// - 若为 Scoped：同一作用域内 ID 相同，不同作用域不同  
    /// - 若为 Transient：每次解析（注入）都产生新实例，ID 不同
    /// </returns>
    public string GetMessage()
    {
        return $"[生命周期测试] 服务实例 ID: {_instanceId}";
    }
}

//调度添加示例
//{
//  "recurringJobId": "1234",
//  "cronExpression": "*/15 * * * * *",
//  "JobClass": "Renova.Core.Jobs.JobDemo,Renova.Core",
//  "Parameters": ""
//}