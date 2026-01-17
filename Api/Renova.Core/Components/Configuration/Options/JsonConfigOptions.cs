using Renova.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Renova.Core.Components.Configuration.Options;

/// <summary>
/// JSON 配置选项
/// </summary>
public class JsonConfigOptions : IConfigSectionProvider
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public static string SectionName => "JsonConfiguration";

    /// <summary>
    /// 是否启用动态 JSON 加载
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 需要加载的目录名
    /// </summary>
    [Required]
    public string Folder { get; set; } = "Config";

    /// <summary>
    /// 包含的文件模式
    /// </summary>
    public string[] Include { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 排除的文件
    /// </summary>
    public string[] Exclude { get; set; } = Array.Empty<string>();
}
