using Microsoft.AspNetCore.Mvc;
using Renova.Core.Components.FileStorage.Providers;
using System.Net.Mime;

namespace Renova.WebApi.Controllers;

/// <summary>
/// 文件接口（测试本地文件上传、下载、预览等）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "文件操作")]

public class FileController : ControllerBase
{
    private readonly IFileStorageProvider _fileStorage;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileStorage"></param>
    public FileController(IFileStorageProvider fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// 上传文件（返回文件路径和可访问 URL）
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, string? folder = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("未选择文件");

        var relativePath = await _fileStorage.UploadAsync(file, folder);
        var fileUrl = _fileStorage.GetFileUrl(relativePath);

        return Ok(new
        {
            filePath = relativePath,
            url = fileUrl
        });
    }

    /// <summary>
    /// 浏览器中打开文件（预览）
    /// </summary>
    [HttpGet("view")]
    public async Task<IActionResult> ViewFile([FromQuery] string filePath)
    {
        var stream = await _fileStorage.GetAsync(filePath);
        if (stream == null) return NotFound();

        var fileName = Path.GetFileName(filePath);
        return File(stream, GetContentType(fileName));

        // or 返回文件地址
        //var fileUrl = _fileStorage.GetFileUrl(filePath);
        //return Ok(new { url = fileUrl });
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string filePath)
    {
        var stream = await _fileStorage.GetAsync(filePath);
        if (stream == null) return NotFound();

        var fileName = Path.GetFileName(filePath);
        return File(stream, MediaTypeNames.Application.Octet, fileName);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] string filePath)
    {
        var result = await _fileStorage.DeleteAsync(filePath);
        return result ? Ok("删除成功") : NotFound("文件不存在");
    }

    private string GetContentType(string fileName)
    {
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }
}
