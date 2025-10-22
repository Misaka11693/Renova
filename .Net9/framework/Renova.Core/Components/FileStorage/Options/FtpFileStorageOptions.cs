namespace Renova.Core.Components.FileStorage.Options;

/// <summary>
/// FTP 文件存储配置选项。
/// </summary>
public class FtpFileStorageOptions
{
    /// <summary>
    /// FTP 服务器主机地址
    /// </summary>
    public string Host { get; set; } = default!;

    /// <summary>
    /// FTP 服务端口，默认为 21（标准 FTP 端口）
    /// </summary>
    public int Port { get; set; } = 21;

    /// <summary>
    /// 登录 FTP 服务器的用户名
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// 登录 FTP 服务器的密码
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// FTP 服务器上的存储根目录
    /// </summary>
    public string StoragePath { get; set; } = "/uploads";

    /// <summary>
    /// 文件的公共访问 URL 前缀，用于生成可通过 HTTP/HTTPS 直接访问的文件链接。
    /// </summary>
    public string PublicUrl { get; set; } = default!;
}