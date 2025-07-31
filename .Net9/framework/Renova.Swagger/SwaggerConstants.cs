using Microsoft.OpenApi.Models;

namespace Renova.Swagger;

public static class SwaggerConstants
{
    public const string AllGroupName = "All";
    public const string DefaultGroupName = "Default";

    /// <summary>
    /// 预注册特殊分组
    /// </summary>
    public static readonly Dictionary<string, OpenApiInfo> ApiDocs = new()
    {
        [AllGroupName] = new OpenApiInfo
        {
            Title = "🌐 全部接口",
            Version = "v1.0.0",
            Description = "系统所有可用接口的完整集合"
        },
        [DefaultGroupName] = new OpenApiInfo
        {
            Title = "⚙️ 默认分组",
            Version = "v1.0.0",
            Description = "未明确指定分组的接口集合"
        }
    };
}
