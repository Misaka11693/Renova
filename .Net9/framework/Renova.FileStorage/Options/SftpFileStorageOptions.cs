namespace Renova.FileStorage.Options;

/// <summary>
/// SFTP 文件存储配置选项
/// </summary>
public class SftpFileStorageOptions
{
    /// <summary>
    /// SFTP 服务器主机地址
    /// </summary>
    public string Host { get; set; } = default!;

    /// <summary>
    /// SFTP 服务端口，默认为 22（标准 SSH/SFTP 端口）
    /// </summary>
    public int Port { get; set; } = 22;

    /// <summary>
    /// 登录 SFTP 服务器的用户名
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// 登录 SFTP 服务器的密码
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// SFTP 服务器上的存储根目录，文件将上传到此路径下
    /// </summary>
    public string StoragePath { get; set; } = "/uploads";

    /// <summary>
    /// 文件的公共访问 URL 前缀，用于生成可通过 HTTP/HTTPS 直接访问的文件链接
    /// </summary>
    public string PublicUrl { get; set; } = default!;
}