using Jaina;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Renova.EventBus.Sources;
using Renova.RabbitMQ.Interfaces;
using System.Text;
using System.Threading.Channels;

namespace Renova.EventBus.Storers;

/// <summary>
///  基于 RabbitMQ 的事件源存储器
/// </summary>
public class RabbitMQEventSourceStorer : IEventSourceStorer, IAsyncDisposable
{
    /// <summary>
    /// 内存通道事件源存储器
    /// </summary>
    private readonly Channel<IEventSource> _memoryChannel;

    /// <summary>
    /// 日志服务
    /// </summary>
    private readonly ILogger<RabbitMQEventSourceStorer> _logger;

    /// <summary>
    /// RabbitMQ 通道
    /// </summary>
    private IChannel? _rabbitMQChannel;

    /// <summary>
    /// RabbitMQ 连接
    /// </summary>
    private readonly IRabbitMQConnection? _rabbitMqConnection;

    /// <summary>
    /// 路由键
    /// </summary>
    private readonly string _routeKey;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="factory">连接工厂</param>
    /// <param name="routeKey">路由键</param>
    /// <param name="capacity">存储器最多能够处理多少消息，超过该容量进入等待写入</param>
    public RabbitMQEventSourceStorer(IRabbitMQConnection rabbitMqConnection, ILogger<RabbitMQEventSourceStorer> logger, string routeKey, int capacity)
    {
        if (rabbitMqConnection == null)
            throw new ArgumentNullException(nameof(rabbitMqConnection));

        if (string.IsNullOrWhiteSpace(routeKey))
            throw new ArgumentException("路由键不能为空");

        // 创建内存通道
        _memoryChannel = Channel.CreateBounded<IEventSource>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        _rabbitMqConnection = rabbitMqConnection;
        _routeKey = routeKey;
        _logger = logger;

        // 初始化连接
        InitializeRabbitMQAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// 初始化 RabbitMQ 连接和订阅者，并开始监听消息队列。
    /// </summary>
    /// <returns></returns>
    public async Task InitializeRabbitMQAsync()
    {
        // 创建 RabbitMQ 通道
        _rabbitMQChannel = await _rabbitMqConnection!.CreateChannelAsync();

        // 声明队列
        await _rabbitMQChannel.QueueDeclareAsync(queue: _routeKey, durable: false, exclusive: false, autoDelete: false, arguments: null);

        // 创建消息订阅者
        var consumer = new AsyncEventingBasicConsumer(_rabbitMQChannel);

        // 订阅消息并写入内存 Channel
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                // 读取原始消息
                var stringEventSource = Encoding.UTF8.GetString(ea.Body.ToArray());

                // 转换为 IEventSource，如果自定义了 EventSource，注意属性是可读可写
                var eventSource = JsonConvert.DeserializeObject<DistributedEventSource>(stringEventSource);

                if (eventSource != null)
                {
                    // 写入内存通道
                    await _memoryChannel!.Writer.WriteAsync(eventSource);
                }

                // 消息确认
                await _rabbitMQChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                // 记录错误
                _logger.LogError(ex, "处理事件源时发生错误");

                // 记录错误并拒绝消息（重新入队）
                await _rabbitMQChannel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }

        };

        // 启动消费者（手动应答）
        await _rabbitMQChannel.BasicConsumeAsync(queue: _routeKey, autoAck: false, consumer: consumer);
    }

    /// <summary>
    /// 将事件源写入存储器
    /// </summary>
    /// <param name="eventSource">事件源对象</param>
    /// <param name="cancellationToken">取消任务 Token</param>
    /// <returns><see cref="ValueTask"/></returns>
    public async ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken)
    {
        // 空检查
        if (eventSource == default)
        {
            throw new ArgumentNullException(nameof(eventSource));
        }

        if (eventSource is ChannelEventSource)
        {
            // 写入存储器
            await _memoryChannel.Writer.WriteAsync(eventSource, cancellationToken);
        }
        else if (eventSource is DistributedEventSource)
        {
            // 序列化事件源为 JSON
            var json = JsonConvert.SerializeObject(eventSource);
            var body = Encoding.UTF8.GetBytes(json);
            var properties = new BasicProperties
            {
                Persistent = true // 设置消息持久化
            };

            // 发布到 RabbitMQ
            await _rabbitMQChannel!.BasicPublishAsync("", _routeKey, false, properties, body, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"不支持的事件类型: {eventSource.GetType()}");
        }
    }

    /// <summary>
    /// 从存储器中读取一条事件源
    /// </summary>
    /// <param name="cancellationToken">取消任务 Token</param>
    /// <returns>事件源对象</returns>
    public async ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken)
    {
        // 读取一条事件源
        var eventSource = await _memoryChannel.Reader.ReadAsync(cancellationToken);
        return eventSource;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_rabbitMQChannel is not null)
        {
            await _rabbitMQChannel.DisposeAsync();
        }
    }
}
