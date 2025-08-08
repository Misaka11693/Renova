using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renova.FileStorage.Options;
using Renci.SshNet;

namespace Renova.FileStorage.Providers.Sftp;

/// <summary>
/// SFTP 文件存储提供器
/// </summary>
public class SftpFileStorageProvider : IFileStorageProvider
{
    private readonly SftpFileStorageOptions _options;
    private readonly ILogger<SftpFileStorageProvider> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SftpFileStorageProvider(
        IOptions<FileStorageOptions> options,
        ILogger<SftpFileStorageProvider> logger)
    {
        _options = options.Value.Sftp ?? throw new ArgumentNullException(nameof(options.Value.Sftp));
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.Host))
            throw new ArgumentException("SFTP 主机地址不能为空");
    }

    /// <summary>
    /// 连接 SFTP 客户端
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private SftpClient CreateAndConnectClient()
    {
        try
        {
            var client = new SftpClient(_options.Host, _options.Port, _options.Username, _options.Password);
            client.Connect();
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SFTP 连接失败");
            throw new InvalidOperationException("无法连接到 SFTP 服务器，请检查配置和网络连接", ex);
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
        var sftpFullPath = CombinePath(_options.StoragePath, relativePath);

        using var client = CreateAndConnectClient();

        var remoteDir = Path.GetDirectoryName(sftpFullPath)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(remoteDir) && !client.Exists(remoteDir))
        {
            CreateDirectoryRecursively(client, remoteDir);
        }

        if (content.CanSeek) content.Position = 0;
        await Task.Run(() => client.UploadFile(content, sftpFullPath, true));

        _logger.LogInformation($"SFTP 文件上传成功: {sftpFullPath}");

        return relativePath;
    }

    /// <summary>
    /// 获取文件的读取流
    /// </summary>
    public async Task<Stream?> GetAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("文件路径不能为空", nameof(filePath));

        var sftpPath = CombinePath(_options.StoragePath, filePath);

        using var client = CreateAndConnectClient();

        if (!client.Exists(sftpPath))
        {
            _logger.LogError($"SFTP 文件不存在: {sftpPath}");
            throw new FileNotFoundException($"文件在服务器上不存在或者已被删除: {sftpPath}");
        }

        var ms = new MemoryStream();
        await Task.Run(() => client.DownloadFile(sftpPath, ms));
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// 删除指定路径的文件
    /// </summary>
    public async Task<bool> DeleteAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("文件路径不能为空", nameof(filePath));

        var sftpPath = CombinePath(_options.StoragePath, filePath);

        using var client = CreateAndConnectClient();

        if (!client.Exists(sftpPath))
        {
            _logger.LogWarning($"删除文件不存在: {sftpPath}");
            return false;
        }

        await Task.Run(() => client.DeleteFile(sftpPath));

        _logger.LogInformation($"SFTP 文件已删除: {sftpPath}");

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

    /// <summary>
    /// 创建远程目录（递归）
    /// </summary>
    private void CreateDirectoryRecursively(SftpClient client, string directory)
    {
        var parts = directory.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var path = string.Empty;

        foreach (var part in parts)
        {
            path += "/" + part;
            if (!client.Exists(path))
            {
                client.CreateDirectory(path);
            }
        }
    }
}
