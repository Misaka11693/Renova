using Microsoft.AspNetCore.Http;

namespace Renova.Core;

/// <summary>
/// Api 业务状态码枚举
/// 只有0表示成功，其余均为失败
/// </summary>
public enum ApiCode
{
    Success = StatusCodes.Status200OK,              // 通用成功
    Failed = 1,               // 通用失败

    Unauthorized = 401,       // 未认证或Token过期
    Forbidden = 403,          // 无权限访问
    NotFound = 404,           // 请求资源不存在

    InvalidArgument = 410,    // 参数错误
    ValidationFailed = 411,   // 数据校验失败

    Conflict = 420,           // 业务冲突（如重复提交）

    InternalError = 500       // 服务器内部错误
}
