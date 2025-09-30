namespace Microsoft.AspNetCore.Http;

/// <summary>
///  Http 扩展方法
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 判断是否是 WebSocket 请求
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool IsWebSocketRequest(this HttpContext context)
    {
        return context.WebSockets.IsWebSocketRequest;
    }
}
