namespace Renova.Core.Components.Cache;

/// <summary>
/// Redis配置选项
/// </summary>
public class RedisOptions : NewLife.Caching.RedisOptions
{
    /// <summary>
    /// 定时任务数据库
    /// </summary>
    public int? JobDb { get; set; }

    /// <summary>
    /// 生成适用于 StackExchange.Redis（Hangfire）的连接字符串
    /// </summary>
    public string? ToStackExchangeConnectionString()
    {
        if (string.IsNullOrEmpty(Server))
            return null;

        var hostPart = Server;

        var options = new List<string>();

        if (!string.IsNullOrEmpty(Password))
            options.Add($"password={Password}");

        if (!string.IsNullOrEmpty(UserName))
            options.Add($"user={UserName}");

        options.Add($"syncTimeout={Timeout}");

        if (options.Count == 0)
            return hostPart;

        return $"{hostPart},{string.Join(",", options)}";
    }
}
