namespace Renova.Core;

/// <summary>
/// 忽略表结构初始化特性（标记在实体）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class IgnoreTableAttribute : Attribute
{
}
