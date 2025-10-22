using OnceMi.AspNetCore.OSS;

namespace Renova.Core.Components.FileStorage.Options;

/// <summary>
/// OSS 文件存储配置选项
/// </summary>
public class OssFileStorageOptions
{
    /// <summary>
    /// OSS 服务提供商
    /// </summary>
    public OSSProvider Provider { get; set; }

    /// <summary>
    /// 服务接入点
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// 区域标识（Region），部分厂商需要指定，如 us-east-1
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// 访问密钥 ID
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// 访问密钥密钥
    /// </summary>
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// 存储桶名称
    /// </summary>
    public string BucketName { get; set; } = "uploads";

    /// <summary>
    /// 文件的公共访问 URL 前缀，用于生成外链地址
    /// </summary>
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用本地缓存，默认为 true
    /// </summary>
    public bool IsEnableCache { get; set; } = true;

    /// <summary>
    /// 是否使用 HTTPS 协议访问，默认为 false（使用 HTTP）
    /// </summary>
    public bool IsEnableHttps { get; set; } = false;
}