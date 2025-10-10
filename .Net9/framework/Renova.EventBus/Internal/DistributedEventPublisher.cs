using Jaina;
using Renova.EventBus.Sources;

namespace Renova.EventBus.Internal;

/// <summary>
/// 基于分布式消息队列的事件发布器
/// </summary>
internal sealed partial class DistributedEventPublisher : IEventPublisher
{
    /// <summary>
    /// 事件处理程序事件
    /// </summary>
    public event EventHandler<EventHandlerEventArgs>? OnExecuted;

    /// <summary>
    /// 事件源存储器
    /// </summary>
    private readonly IEventSourceStorer _eventSourceStorer;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventSourceStorer">事件源存储器</param>
    public DistributedEventPublisher(IEventSourceStorer eventSourceStorer)
    {
        _eventSourceStorer = eventSourceStorer;
    }

    /// <summary>
    /// 发布一条消息
    /// </summary>
    /// <param name="eventSource">事件源</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public async Task PublishAsync(IEventSource eventSource)
    {
        if (string.IsNullOrEmpty(eventSource.EventId))
        {
            throw new ArgumentNullException(nameof(eventSource.EventId));
        }
        if (eventSource.Payload is null)
        {
            throw new ArgumentNullException(nameof(eventSource.Payload));
        }

        await _eventSourceStorer.WriteAsync(eventSource, eventSource.CancellationToken);
    }

    /// <summary>
    /// 延迟发布一条消息
    /// </summary>
    /// <param name="eventSource">事件源</param>
    /// <param name="delay">延迟数（毫秒）</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public Task PublishDelayAsync(IEventSource eventSource, long delay)
    {
        if (string.IsNullOrEmpty(eventSource.EventId))
        {
            throw new ArgumentNullException(nameof(eventSource.EventId));
        }
        if (eventSource.Payload is null)
        {
            throw new ArgumentNullException(nameof(eventSource.Payload));
        }

        // 创建新线程
        Task.Factory.StartNew(async () =>
        {
            // 延迟 delay 毫秒
            await Task.Delay(TimeSpan.FromMilliseconds(delay), eventSource.CancellationToken);

            await _eventSourceStorer.WriteAsync(eventSource, eventSource.CancellationToken);
        }, eventSource.CancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns></returns>
    public async Task PublishAsync(string eventId, object? payload = default, CancellationToken cancellationToken = default)
    {
        await PublishAsync(new DistributedEventSource(eventId, payload, cancellationToken));
    }

    /// <summary>
    /// 发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns></returns>
    public async Task PublishAsync(Enum eventId, object? payload = default, CancellationToken cancellationToken = default)
    {
        await PublishAsync(new DistributedEventSource(eventId, payload, cancellationToken));
    }

    /// <summary>
    /// 延迟发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="delay">延迟数（毫秒）</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public async Task PublishDelayAsync(string eventId, long delay, object? payload = default, CancellationToken cancellationToken = default)
    {
        await PublishDelayAsync(new DistributedEventSource(eventId, payload, cancellationToken), delay);
    }

    /// <summary>
    /// 延迟发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="delay">延迟数（毫秒）</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public async Task PublishDelayAsync(Enum eventId, long delay, object? payload = default, CancellationToken cancellationToken = default)
    {
        await PublishDelayAsync(new DistributedEventSource(eventId, payload, cancellationToken), delay);
    }

    /// <summary>
    /// 触发事件处理程序事件
    /// </summary>
    /// <param name="args">事件参数</param>
    public void InvokeEvents(EventHandlerEventArgs args)
    {
        try
        {
            OnExecuted?.Invoke(this, args);
        }
        catch { }
    }
}
