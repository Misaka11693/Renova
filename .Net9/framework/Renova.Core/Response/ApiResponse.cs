namespace Renova.Core.Response;

/// <summary>
/// 统一 API 响应模型 
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 响应状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应状态信息
    /// </summary>
    public object? Message { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 返回服务器当前时间
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.Now;

    public ApiResponse() { }

    public ApiResponse(int code, string? message = null, object? data = null)
    {
        Code = code;
        Message = message;
        Data = data;
    }
}

