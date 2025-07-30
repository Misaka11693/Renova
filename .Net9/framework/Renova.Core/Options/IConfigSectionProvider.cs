namespace Renova.Core;

/// <summary>
/// 配置节名称提供者接口
/// </summary>
public interface IConfigSectionProvider
{
    /// <summary>
    /// 获取配置节名称
    /// </summary>
    public static abstract string SectionName { get; }
}
