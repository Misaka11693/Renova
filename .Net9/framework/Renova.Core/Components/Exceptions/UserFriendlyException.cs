namespace System;

/// <summary>
/// 用户友好异常
/// </summary>
public class UserFriendlyException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public UserFriendlyException()
    {
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    public UserFriendlyException(string message)
        : base(message)
    {
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public UserFriendlyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
