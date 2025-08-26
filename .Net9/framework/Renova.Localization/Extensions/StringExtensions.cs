using Renova.Core;
using Renova.Localization.Abstractions;

namespace System;

/// <summary>
/// 字符串本地化扩展方法
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 将当前字符串作为本地化 key，获取对应的本地化文本
    /// </summary>
    /// <param name="key">本地化 key</param>
    /// <param name="culture">可选的文化代码（如 "en-US"、"ja-JP"），不传则使用当前线程文化</param>
    /// <returns>本地化后的字符串</returns>
    public static string L10N(this string key, string? culture = null)
    {
        var service = App.GetRequiredService<ILocalizationService>();
        return service.GetString(key, culture);
    }

    /// <summary>
    /// 将当前字符串作为本地化 key，获取对应的本地化文本，并使用指定的参数进行格式化（使用当前线程文化）
    /// </summary>
    /// <param name="key">本地化 key</param>
    /// <param name="args">格式化参数</param>
    /// <returns>本地化并格式化后的字符串</returns>
    /// <example>
    /// <code>
    /// var message = "当前时间是 {0}".L10NFormat(DateTime.Now);
    /// </code>
    /// </example>
    public static string L10NFormat(this string key, params object[] args)
    {
        return string.Format(key.L10N(), args);
    }

    /// <summary>
    /// 将当前字符串作为本地化 key，获取对应的本地化文本，并使用指定的参数进行格式化（指定文化）
    /// </summary>
    /// <param name="key">本地化 key</param>
    /// <param name="culture">文化代码（如 "en-US"、"ja-JP"）</param>
    /// <param name="args">格式化参数</param>
    /// <returns>本地化并格式化后的字符串</returns>
    /// <example>
    /// <code>
    /// var message = "当前时间是 {0}".L10NFormat("en-US", DateTime.Now);
    /// </code>
    /// </example>
    //public static string L10NFormat(this string key, string culture, params object[] args)
    //{
    //    return string.Format(key.L10N(culture), args);
    //}
}
