using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core;

/// <summary>
/// API 业务状态码枚举
/// </summary>
public enum ApiCode
{
    Success = 0,              // ✅ 成功
    Failed = 1,               // ❌ 通用失败

    Unauthorized = 401,       // 未认证或Token过期
    Forbidden = 403,          // 无权限访问
    NotFound = 404,           // 请求资源不存在

    InvalidArgument = 410,    // 参数错误
    ValidationFailed = 411,   // 数据校验失败

    Conflict = 420,           // 业务冲突（如重复提交）

    InternalError = 500       // 服务器内部错误
}
