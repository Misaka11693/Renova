namespace Renova.Core.Components.Localization;

/// <summary>
/// 本地化服务接口
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// 根据 key 获取指定文化的本地化文本
    /// </summary>
    /// <param name="key">资源键</param>
    /// <param name="culture">文化代码（如 zh-CN、en-US），为空则使用当前 UI 文化</param>
    /// <returns>本地化后的字符串，如果找不到则返回 key 本身</returns>
    string GetString(string key, string? culture = null);
}
