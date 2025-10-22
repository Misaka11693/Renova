using FluentFTP;
using FluentFTP.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renova.Core.Components.FileStorage.Options;
using Renova.Core.Components.FileStorage.Providers;

namespace Renova.Core.Components.FileStorage.Providers.Ftp;

/// <summary>
/// FluentFTP 文件存储提供器
/// https://github.com/robinrodricks/FluentFTP/wiki/Quick-Start-Example
/// </summary>
public class FtpFileStorageProvider : IFileStorageProvider
{
    private readonly FtpFileStorageOptions _options;
    private readonly ILogger<FtpFileStorageProvider> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public FtpFileStorageProvider(
        IOptions<FileStorageOptions> options,
        ILogger<FtpFileStorageProvider> logger)
    {
        _options = options.Value.Ftp ?? throw new ArgumentNullException(nameof(options.Value.Ftp));
        _logger = logger;

        //匿名登录时账号密码可为空
        if (string.IsNullOrWhiteSpace(_options.Host))
            throw new ArgumentException("FTP 主机地址不能为空");
    }

    /// <summary>
    /// 连接FTP客户端
    /// </summary>
    private async Task<AsyncFtpClient> CreateAndConnectClientAsync()
    {
        try
        {
            var client = new AsyncFtpClient(_options.Host, _options.Username, _options.Password, _options.Port);
            await client.AutoConnect();
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP 连接失败");
            throw new InvalidOperationException("无法连接到 FTP 服务器，请检查配置和网络连接", ex);
        }
    }

    /// <summary>
    /// 上传文件，返回文件相对路径（如：docs/abc.pdf）
    /// </summary>
    public async Task<string> UploadAsync(IFormFile file, string? folder = null)
    {
        using var stream = file.OpenReadStream();
        return await UploadAsync(file.FileName, stream, folder);
    }

    /// <summary>
    /// 上传任意文件流，返回文件相对路径（如：docs/abc.pdf）
    /// </summary>
    public async Task<string> UploadAsync(string fileName, Stream content, string? folder = null)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new ArgumentException("无效的文件名", nameof(fileName));

        var guidFolder = Guid.NewGuid().ToString("N");

        var relativeFolder = string.IsNullOrWhiteSpace(folder)
            ? guidFolder
            : $"{folder.Trim('/')}/{guidFolder}";

        var relativePath = $"{relativeFolder}/{safeFileName}".Replace('\\', '/');
        var ftpFullPath = CombinePath(_options.StoragePath, relativePath);

        await using var client = await CreateAndConnectClientAsync();

        var remoteDir = Path.GetDirectoryName(ftpFullPath)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(remoteDir))
        {
            await client.CreateDirectory(remoteDir, true);
        }

        if (content.CanSeek) content.Position = 0;
        var result = await client.UploadStream(content, ftpFullPath, FtpRemoteExists.Overwrite, true);
        if (result.IsFailure())
        {
            _logger.LogError($"FTP 上传失败：{result}, 路径: {ftpFullPath}");
            throw new InvalidOperationException($"文件上传失败: {fileName}");
        }

        _logger.LogInformation($"文件上传成功: {ftpFullPath}");
        return relativePath;
    }

    /// <summary>
    /// 获取文件的读取流
    /// </summary>
    public async Task<Stream?> GetAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        var ftpPath = CombinePath(_options.StoragePath, filePath);
        await using var client = await CreateAndConnectClientAsync();

        if (!await client.FileExists(ftpPath))
        {
            _logger.LogError($"文件不存在: {ftpPath}");
            throw new FileNotFoundException($"文件在服务器上不存在或者已被删除: {ftpPath}");
        }

        var memoryStream = new MemoryStream();
        var success = await client.DownloadStream(memoryStream, ftpPath);

        if (!success)
        {
            _logger.LogError($"文件下载失败: {ftpPath}");
            throw new IOException($"文件下载失败: {filePath}");
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// 删除指定路径的文件
    /// </summary>
    public async Task<bool> DeleteAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        var ftpPath = CombinePath(_options.StoragePath, filePath);
        await using var client = await CreateAndConnectClientAsync();

        var exists = await client.FileExists(ftpPath);
        if (!exists)
        {
            _logger.LogWarning($"删除文件不存在: {ftpPath}");
            return false;
        }

        await client.DeleteFile(ftpPath);

        _logger.LogInformation($"文件已删除: {ftpPath}");

        return true;
    }

    /// <summary>
    /// 获取文件的公网访问 URL
    /// </summary
    public string GetFileUrl(string filePath)
    {
        return $"{_options.PublicUrl.TrimEnd('/')}/{filePath.TrimStart('/')}";
    }

    /// <summary>
    /// 合并基础路径和相对路径
    /// </summary>
    private string CombinePath(string basePath, string relativePath)
    {
        return $"{basePath.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }
}
