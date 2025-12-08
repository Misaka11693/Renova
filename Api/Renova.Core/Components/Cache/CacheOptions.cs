using NewLife.Caching;
using Renova.Core.Common;

namespace Renova.Core.Components.Cache;

/// <summary>
/// 缓存配置选项
/// </summary>
public sealed class CacheOptions : IConfigSectionProvider
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public static string SectionName => "CacheOptions";

    /// <summary>
    /// 缓存类型
    /// </summary>
    public string? CacheType { get; set; }

    /// <summary>
    /// Redis缓存
    /// </summary>
    public RedisOptions? Redis { get; set; }
}

