using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Renova.RabbitMQ.Interfaces;
using Renova.RabbitMQ.Options;

namespace Renova.RabbitMQ;

/// <summary>
/// RabbitMQ 连接管理器
/// </summary>
public class RabbitMQConnection : IRabbitMQConnection, IAsyncDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQConnection> _logger;
    private IConnection? _connection;
    private bool _disposed;

    // 用于保护连接创建/重连操作的并发访问
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    // 重连相关状态
    private int _reconnectAttempts = 0;
    private const int MaxReconnectAttempts = 5; // 最多重试5次

    /// <summary>
    /// 当前连接是否有效且未释放
    /// </summary>
    public bool IsConnected => _connection?.IsOpen == true && !_disposed;

    /// <summary>
    /// 构造函数
    /// </summary>
    public RabbitMQConnection(
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 尝试建立 RabbitMQ 连接（线程安全）
    /// </summary>
    public async Task<bool> TryConnectAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            // 已连接或已释放，直接返回
            if (IsConnected || _disposed)
                return IsConnected;

            _logger.LogInformation("正在尝试连接到 RabbitMQ...");

            // 配置连接工厂
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,           // 启用自动恢复（作为兜底）
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(60) // 心跳检测，避免假死
            };

            // 创建连接
            _connection = await factory.CreateConnectionAsync();

            // 注册关键事件（用于监控连接状态）
            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
            _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
            _connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;

            // 重置重连计数
            _reconnectAttempts = 0;
            _logger.LogInformation("RabbitMQ 连接已成功建立");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ 连接失败");
            return false; // 不抛异常，由调用方决定是否重试或报错
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    #region 连接事件处理

    /// <summary>
    /// 连接关闭事件处理
    /// </summary>
    private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        if (e.Initiator == ShutdownInitiator.Application)
        {
            _logger.LogInformation("应用程序主动关闭了 RabbitMQ 连接");
            return;
        }

        _logger.LogWarning("RabbitMQ 连接意外关闭: {Reason}", e.ReplyText);
        await TryReconnectAsync();
    }

    /// <summary>
    /// 回调异常事件处理
    /// </summary>
    private async Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
    {
        _logger.LogWarning(e.Exception, "RabbitMQ 回调异常");
        await TryReconnectAsync();
    }

    /// <summary>
    /// 连接被阻塞事件处理（如内存不足）
    /// </summary>
    private async Task OnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogWarning("RabbitMQ 连接被阻塞: {Reason}", e.Reason);
        await TryReconnectAsync();
    }

    #endregion

    /// <summary>
    /// 尝试重连（带指数退避）
    /// </summary>
    private async Task TryReconnectAsync()
    {
        if (_disposed || _reconnectAttempts >= MaxReconnectAttempts)
        {
            _logger.LogError("RabbitMQ 重连已达最大次数 ({Max})，停止重试", MaxReconnectAttempts);
            return;
        }

        // 指数退避：1s, 2s, 4s, 8s, 16s...
        var delay = TimeSpan.FromSeconds(Math.Pow(2, _reconnectAttempts));
        _logger.LogWarning("等待 {DelaySeconds} 秒后尝试第 {Attempt} 次重连...", delay.TotalSeconds, _reconnectAttempts + 1);
        await Task.Delay(delay);

        if (await TryConnectAsync())
        {
            _logger.LogInformation("RabbitMQ 重连成功");
        }
        else
        {
            _reconnectAttempts++;
            _logger.LogWarning("RabbitMQ 重连失败，当前重试次数: {Attempts}", _reconnectAttempts);
        }
    }

    /// <summary>
    /// 创建一个新的通道（Channel）
    /// 如果未连接，会自动尝试连接
    /// </summary>
    public async Task<IChannel> CreateChannelAsync()
    {
        if (!IsConnected)
        {
            if (!await TryConnectAsync())
            {
                throw new InvalidOperationException("无法建立 RabbitMQ 连接，请检查配置或网络");
            }
        }

        try
        {
            var channel = await _connection!.CreateChannelAsync();
            _logger.LogDebug("RabbitMQ 通道已创建");
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建 RabbitMQ 通道失败");
            throw;
        }
    }

    /// <summary>
    /// 异步释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await _connectionLock.WaitAsync();
        try
        {
            if (_disposed) return;
            _disposed = true;

            if (_connection != null)
            {
                // 先关闭连接（触发正常关闭流程）
                if (_connection.IsOpen)
                {
                    try
                    {
                        await _connection.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "关闭 RabbitMQ 连接时发生异常");
                    }
                }

                // 再注销事件（避免在 Dispose 过程中触发回调）
                _connection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
                _connection.CallbackExceptionAsync -= OnCallbackExceptionAsync;
                _connection.ConnectionBlockedAsync -= OnConnectionBlockedAsync;

                // 释放连接资源
                await _connection.DisposeAsync();
                _logger.LogInformation("RabbitMQ 连接已成功释放");
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}