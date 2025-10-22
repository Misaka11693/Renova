using My.Extensions.Localization.Json;
using Renova.Core.Common;
using System.Reflection;

namespace Renova.Core.Components.Localization;

/// <summary>
/// 本地化配置选项
/// </summary>
public class LocalizationOptions : IConfigSectionProvider
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public static string SectionName => "Localization";

    /// <summary>
    /// 资源文件格式
    /// 可选值：
    /// - "json"：使用 JSON 文件作为多语言资源
    /// - "resx"：使用 RESX 文件作为多语言资源
    /// - "db"：从数据库加载多语言资源
    /// </summary>
    public string ProviderType { get; set; } = "json";

    /// <summary>
    /// JSON 资源文件组织方式（CultureBased / TypeBased）
    /// </summary>
    public ResourcesType JsonResourcesType { get; set; } = ResourcesType.CultureBased;

    /// <summary>
    /// 资源文件所在的相对路径
    /// 相对于项目根目录，例如 "Resources"
    /// </summary>
    public string ResourcesPath { get; set; } = "Resources";

    /// <summary>
    /// 资源文件的基础文件名（不包含语言后缀和扩展名）
    /// 例如：
    /// - 文件名为 lang.zh-CN.json，则 ResourceFileName = "lang"
    /// - 文件名为 lang.en-US.resx，则 ResourceFileName = "lang"
    /// </summary>
    public string ResourceFileName { get; set; } = "lang";

    /// <summary>
    /// 资源所在程序集名称
    /// 用于 RESX 或嵌入式资源定位
    /// 例如：
    /// - "Renova.WebApi"
    /// - "Renova.Localization"
    /// </summary>
    public string ResourceAssemblyName { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new InvalidOperationException("无法获取默认的 ResourceAssemblyName");

    /// <summary>
    /// 默认文化（语言-地区）
    /// 例如：
    /// - "zh-CN" 表示简体中文（中国）
    /// - "en-US" 表示英语（美国）
    /// </summary>
    public string DefaultCulture { get; set; } = "zh-CN";

    /// <summary>
    /// 支持的文化列表
    /// 例如：
    /// [ "zh-CN", "en-US", "ja-JP" ]
    /// </summary>
    public string[] SupportedCultures { get; set; } = Array.Empty<string>();
}
