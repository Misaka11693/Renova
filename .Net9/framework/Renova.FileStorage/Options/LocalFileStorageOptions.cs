namespace Renova.FileStorage.Options;

/// <summary>
/// 本地文件存储配置选项
/// </summary>
public class LocalFileStorageOptions
{
    /// <summary>
    /// 本地存储根路径，为 wwwroot 目录下的子目录
    /// </summary>
    public string RootPath { get; set; } = "uploads";
}
