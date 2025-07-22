using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renova.RabbitMQ.Interfaces;

public interface IRabbitMQConnection
{
    /// <summary>
    /// 创建 RabbitMQ 通道
    /// </summary>
    Task<IChannel> CreateChannelAsync();

    /// <summary>
    /// 连接状态
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 尝试重新连接
    /// </summary>
    Task<bool> TryConnectAsync();
}
