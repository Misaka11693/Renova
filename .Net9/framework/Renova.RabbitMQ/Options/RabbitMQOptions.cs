using Renova.Core;

namespace Renova.RabbitMQ.Options;

/// <summary>
/// RabbitMQ 连接配置选项
/// </summary>
public class RabbitMQOptions : IConfigSectionProvider
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public static string SectionName => "RabbitMQOptions";

    /// <summary>
    /// RabbitMQ 主机地址，默认 localhost
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// 端口号，默认 5672
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 登录用户名，默认 guest
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// 登录密码，默认 guest
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// 虚拟主机路径，默认 /
    /// </summary>
    public string VirtualHost { get; set; } = "/";
}
