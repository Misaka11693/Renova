using Microsoft.Extensions.DependencyInjection;
using OnceMi.AspNetCore.OSS;
using Renova.Core;
using Renova.FileStorage.Options;
using Renova.FileStorage.Providers.Ftp;
using Renova.FileStorage.Providers.Local;
using Renova.FileStorage.Providers.Oss;
using Renova.FileStorage.Providers.Sftp;

namespace Renova.FileStorage.Extensions;

/// <summary>
/// 文件存储服务注册扩展类
/// 目前支持：
/// Local（本地磁盘）
/// FTP
/// SFTP
/// OSS（支持Minio、Aliyun、QCloud、Qiniu、HuaweiCloud、BaiduCloud、Ctyun）
/// </summary>
public static class FileStorageServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorageSetup(this IServiceCollection services)
    {
        //注册选项
        services.AddOptions<FileStorageOptions>()
            .BindConfiguration(FileStorageOptions.SectionName)
            .ValidateDataAnnotations();

        //获取配置选项
        var fileStorageOptions = App.GetOptions<FileStorageOptions>();

        if (fileStorageOptions.Provider.Equals("local", StringComparison.OrdinalIgnoreCase))
        {
            //使用本地磁盘存储
            services.AddSingleton<IFileStorageProvider, LocalFileStorageProvider>();
        }
        else if (fileStorageOptions.Provider.Equals("ftp", StringComparison.OrdinalIgnoreCase))
        {
            //使用 FTP 存储
            services.AddSingleton<IFileStorageProvider, FtpFileStorageProvider>();
        }
        else if (fileStorageOptions.Provider.Equals("sftp", StringComparison.OrdinalIgnoreCase))
        {
            //使用 SFTP 存储
            services.AddSingleton<IFileStorageProvider, SftpFileStorageProvider>();
        }
        else if (fileStorageOptions.Provider.Equals("oss", StringComparison.OrdinalIgnoreCase))
        {
            //使用 OSS 存储
            //https://github.com/oncemi/OnceMi.AspNetCore.OSS
            services.AddOSSService(option =>
            {
                var ossOptions = fileStorageOptions.Oss;
                option.Provider = ossOptions.Provider;
                option.Endpoint = ossOptions.Endpoint;
                option.AccessKey = ossOptions.AccessKeyId;
                option.SecretKey = ossOptions.AccessKeySecret;
                option.Region = ossOptions.Region;
                option.IsEnableHttps = ossOptions.IsEnableHttps;
                option.IsEnableCache = ossOptions.IsEnableCache;
            });
            services.AddSingleton<IFileStorageProvider, OssFileStorageProvider>();
        }
        else
        {
            throw new NotSupportedException($"不支持当前文件存储提供商: {fileStorageOptions.Provider}");
        }
        return services;
    }
}