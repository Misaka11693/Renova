using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Renova.RabbitMQ.Interfaces;
using Renova.RabbitMQ.Options;

namespace Renova.RabbitMQ;

public class RabbitMQConnection : IRabbitMQConnection, IAsyncDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQConnection> _logger;
    private IConnection? _connection;
    private bool _disposed;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    public bool IsConnected => _connection?.IsOpen == true && !_disposed;


    public RabbitMQConnection(
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQConnection> logger)
    {
        _options = options.Value;
        _logger = logger;

        _ = TryConnectAsync();
    }


    public async Task<bool> TryConnectAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (IsConnected) return true;

            _logger.LogInformation("正在尝试连接到RabbitMQ...");

            // 创建连接工厂
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true, // 自动恢复连接
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            // 创建新连接
            _connection = await factory.CreateConnectionAsync();


            // 注册异步事件
            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;// 连接关闭事件
            _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;// 回调异常事件
            _connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;// 连接阻塞事件

            _logger.LogInformation("RabbitMQ连接已建立");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ连接失败");
            throw new InvalidOperationException("RabbitMQ连接失败", ex);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        if (e.Initiator == ShutdownInitiator.Application)
        {
            _logger.LogInformation("应用程序关闭了RabbitMQ连接");
            return;
        }

        _logger.LogWarning("RabbitMQ连接意外关闭: {Reason}", e.ReplyText);
        await TryReconnectAsync();
    }

    private async Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
    {
        _logger.LogWarning(e.Exception, "RabbitMQ连接回调异常");
        await TryReconnectAsync();
    }

    private async Task OnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogWarning("RabbitMQ连接被阻塞: {Reason}", e.Reason);
        await TryReconnectAsync();
    }

    private async Task TryReconnectAsync()
    {
        if (_disposed) return;

        try
        {
            // 简单重连，不包含退避逻辑
            await TryConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重连尝试失败");
        }
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        if (!IsConnected && !await TryConnectAsync())
        {
            throw new InvalidOperationException("没有可用的RabbitMQ连接");
        }

        try
        {
            var channel = await _connection!.CreateChannelAsync();
            _logger.LogDebug("RabbitMQ通道已创建");
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通道失败");
            throw;
        }
    }

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
                // 注销异步事件
                _connection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
                _connection.CallbackExceptionAsync -= OnCallbackExceptionAsync;
                _connection.ConnectionBlockedAsync -= OnConnectionBlockedAsync;

                // 异步关闭和释放
                if (_connection.IsOpen)
                {
                    try
                    {
                        await _connection.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "关闭RabbitMQ连接时出错");
                    }
                }

                await _connection.DisposeAsync();
                _logger.LogInformation("RabbitMQ连接已释放");
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}