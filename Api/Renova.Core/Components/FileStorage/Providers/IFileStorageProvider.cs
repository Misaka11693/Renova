using Microsoft.AspNetCore.Http;

namespace Renova.Core.Components.FileStorage.Providers;

/// <summary>
/// 文件存储提供器接口，定义统一的文件操作契约
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>
    /// 上传 Web 文件（IFormFile），返回文件相对路径（如：docs/abc.pdf）
    /// </summary>
    /// <param name="file">上传的表单文件</param>
    /// <param name="folder">可选：存储子目录（如 "images"）</param>
    Task<string> UploadAsync(IFormFile file, string? folder = null);

    /// <summary>
    /// 上传任意文件流，返回文件相对路径（如：docs/abc.pdf）
    /// </summary>
    /// <param name="fileName">文件名（应包含扩展名）</param>
    /// <param name="content">文件内容流</param>
    /// <param name="folder">可选：存储子目录</param>
    Task<string> UploadAsync(string fileName, Stream content, string? folder = null);

    /// <summary>
    /// 删除指定路径的文件
    /// </summary>
    /// <param name="filePath">文件相对路径</param>
    Task<bool> DeleteAsync(string filePath);

    /// <summary>
    /// 获取文件的读取流（用于下载或读取）
    /// </summary>
    /// <param name="filePath">文件相对路径</param>
    /// <returns>返回文件流，若文件不存在则为 null</returns>
    Task<Stream?> GetAsync(string filePath);

    /// <summary>
    /// 获取文件的 Web 访问地址（供前端展示或下载）
    /// </summary>
    /// <param name="filePath">文件相对路径</param>
    /// <returns>完整访问 URL（如https://cdn.xxx.com/abc.jpg）</returns>
    string GetFileUrl(string filePath);
}
