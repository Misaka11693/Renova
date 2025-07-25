using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core;

/// <summary>
/// 统一 API 响应模型
/// </summary>
public class ApiResponse<T>
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
    public T? Data { get; set; }

    ///// <summary>
    ///// 表示是否成功
    ///// </summary>
    //public bool Success => Code == (int)ApiCode.Success;

    /// <summary>
    /// 返回服务器当前时间
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.Now;

    public ApiResponse()
    {
    }

    public ApiResponse(int code, string message, T? data)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    /// <summary>
    /// 返回成功结果
    /// </summary>
    public static ApiResponse<T> Success(T data, string message = "成功") =>
        new ApiResponse<T> { Code = (int)ApiCode.Success, Message = message, Data = data };

    /// <summary>
    /// 返回失败结果
    /// </summary>
    public static ApiResponse<T> Fail(string message = "失败", ApiCode code = ApiCode.Failed) =>
        new ApiResponse<T> { Code = (int)code, Message = message };
}
