using Renova.Core.Common;

namespace Renova.Core.Components.FileStorage.Options;

/// <summary>
///  文件存储配置选项
/// </summary>
public class FileStorageOptions : IConfigSectionProvider
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public static string SectionName => "FileStorage";

    /// <summary>
    /// 当前使用的存储提供商名称（Local、Ftp、Oss）
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// 本地文件存储配置
    /// </summary>
    public LocalFileStorageOptions Local { get; set; } = new();

    /// <summary>
    /// FTP 存储配置
    /// </summary>
    public FtpFileStorageOptions Ftp { get; set; } = new();

    /// <summary>
    /// SFTP 存储配置
    /// </summary>
    public SftpFileStorageOptions Sftp { get; set; } = new();

    /// <summary>
    /// OSS 存储配置（支持 Aliyun、MinIO 等多厂商）
    /// </summary>
    public OssFileStorageOptions Oss { get; set; } = new();
}
