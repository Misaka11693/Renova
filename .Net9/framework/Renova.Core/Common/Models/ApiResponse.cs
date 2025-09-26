namespace Renova.Core;

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
    public string? Message { get; set; }

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

    /// <summary>
    /// 返回成功结果
    /// </summary>
    public static ApiResponse Success(object? data = null)
        => new ApiResponse((int)ApiCode.Success, "请求成功", data);

    /// <summary>
    /// 返回错误结果
    /// </summary>
    public static ApiResponse Error(object? data = null, int code = (int)ApiCode.Failed, string message = "操作失败")
        => new ApiResponse(code, message, data);
}

