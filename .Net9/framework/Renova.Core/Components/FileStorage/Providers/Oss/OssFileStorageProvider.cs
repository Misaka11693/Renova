using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnceMi.AspNetCore.OSS;
using Renova.Core.Components.FileStorage.Options;
using Renova.Core.Components.FileStorage.Providers;

namespace Renova.Core.Components.FileStorage.Providers.Oss;

/// <summary>
///  OSS 文件存储提供器 
/// </summary>
public class OssFileStorageProvider : IFileStorageProvider
{
    private readonly ILogger<OssFileStorageProvider> _logger;
    private readonly IOSSService _ossService;
    private readonly string _bucketName; // OSS 存储桶名称
    private readonly string _publicUrl;  // 公网访问域名

    /// <summary>
    /// 构造函数
    /// </summary>
    public OssFileStorageProvider(
        ILogger<OssFileStorageProvider> logger,
        IOSSService ossService,
        IOptions<FileStorageOptions> options)
    {
        _logger = logger;
        _ossService = ossService;
        var ossOptions = options.Value.Oss ?? throw new ArgumentNullException(nameof(options.Value.Oss));
        _bucketName = ossOptions.BucketName ?? throw new ArgumentNullException(nameof(ossOptions.BucketName));
        _publicUrl = ossOptions.PublicUrl?.TrimEnd('/') ?? throw new ArgumentException("PublicUrl is required for OSS provider");
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
        try
        {
            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName))
                throw new ArgumentException("无效的文件名", nameof(fileName));

            var guid = Guid.NewGuid().ToString("N");

            var objectName = string.IsNullOrEmpty(folder)
                ? $"{guid}/{safeFileName}"
                : $"{folder.TrimEnd('/')}/{guid}/{safeFileName}";

            if (content.CanSeek) content.Position = 0;

            await _ossService.PutObjectAsync(_bucketName, objectName, content);
            return objectName;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("文件上传失败", ex);
        }
    }

    /// <summary>
    /// 获取文件的读取流
    /// </summary>
    public async Task<Stream?> GetAsync(string filePath)
    {
        try
        {
            var memoryStream = new MemoryStream();
            await _ossService.GetObjectAsync(_bucketName, filePath, async stream =>
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
            });

            return memoryStream;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"下载文件失败: {filePath}", ex);
        }
    }

    /// <summary>
    /// 删除指定路径的文件
    /// </summary>
    public async Task<bool> DeleteAsync(string filePath)
    {
        try
        {
            await _ossService.RemoveObjectAsync(_bucketName, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 获取文件的公网访问 URL
    /// </summary
    public string GetFileUrl(string filePath)
    {
        return $"{_publicUrl}/{filePath.TrimStart('/')}";
    }
}