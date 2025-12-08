using Jaina;

namespace Renova.Core.Components.EventBus.Sources;

/// <summary>
/// 内存通道事件源（事件承载对象）
/// </summary>
public sealed class DistributedEventSource : IEventSource
{
    /// <summary>
    /// 事件 Id
    /// </summary>
    public string? EventId { get; set; }

    /// <summary>
    /// 事件承载（携带）数据
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// 事件创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 消息是否只消费一次
    /// </summary>
    public bool IsConsumOnce { get; set; }

    /// <summary>
    /// 取消任务 Token
    /// </summary>
    /// <remarks>用于取消本次消息处理</remarks>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public DistributedEventSource()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    public DistributedEventSource(string? eventId)
    {
        EventId = eventId;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    public DistributedEventSource(string? eventId, object? payload)
        : this(eventId)
    {
        Payload = payload;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken">取消任务 Token</param>
    public DistributedEventSource(string? eventId, object? payload, CancellationToken cancellationToken)
        : this(eventId, payload)
    {
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    public DistributedEventSource(Enum eventId)
        : this(eventId.ParseToString())
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    public DistributedEventSource(Enum eventId, object? payload)
        : this(eventId.ParseToString(), payload)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken">取消任务 Token</param>
    public DistributedEventSource(Enum eventId, object? payload, CancellationToken cancellationToken)
        : this(eventId.ParseToString(), payload, cancellationToken)
    {
    }
}
