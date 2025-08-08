using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Renova.FileStorage.Options;

namespace Renova.FileStorage.Providers.Local;

/// <summary>
/// 本地文件存储提供器
/// </summary>
public class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LocalFileStorageOptions _options;
    private readonly string _basePath;
    private readonly string _baseRequestPath;

    /// <summary>
    /// 构造函数
    /// </summary>
    public LocalFileStorageProvider(
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor,
        IOptions<FileStorageOptions> options)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _options = options.Value.Local ?? throw new ArgumentNullException(nameof(options.Value.Local));

        // 使用 WebRootPath 作为 wwwroot 物理路径，如果不存在，则fallback到ContentRootPath + "wwwroot"
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        // 组合本地文件保存目录（物理路径）
        // 例如：C:\项目路径\wwwroot\uploads
        var rootFolder = _options.RootPath ?? "uploads";
        _basePath = Path.Combine(webRoot, rootFolder);

        // URL 访问路径（相对于网站根目录），例如 "/uploads"
        _baseRequestPath = "/" + rootFolder.TrimStart('/');

        // 确保基础目录存在
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    /// <summary>
    /// 上传文件，返回文件相对路径（如：docs/abc.pdf）
    /// </summary>
    public async Task<string> UploadAsync(IFormFile file, string? folder = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("文件为空", nameof(file));

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

        var saveFolder = Path.Combine(_basePath, relativeFolder);
        var savePath = Path.Combine(saveFolder, safeFileName);

        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }

        using var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fs);

        return relativePath;
    }

    /// <summary>
    /// 获取文件的读取流
    /// </summary>
    public Task<Stream?> GetAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    /// <summary>
    /// 删除指定路径的文件
    /// </summary>
    public Task<bool> DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// 获取文件的公网访问 URL
    /// </summary>
    public string GetFileUrl(string filePath)
    {
        filePath = filePath.Replace("\\", "/").TrimStart('/');
        var relativeUrl = $"{_baseRequestPath}/{filePath}".Replace("//", "/");

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return relativeUrl;

        return $"{request.Scheme}://{request.Host.Value}{relativeUrl}";
    }
}
