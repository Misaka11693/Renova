using Elastic.CommonSchema.Serilog;
using Microsoft.AspNetCore.Http;

namespace Renova.Core.Components.Serilog.Sinks.Elasticsearch.Ecs
{
    /// <summary>
    ///  ECS 格式化配置
    /// </summary>
    public class EcsFormatterConfig : EcsTextFormatterConfiguration<MyEcsDocument>
    {
        /// <summary>
        /// ASP.NET Core HTTP 访问器
        /// </summary>
        private readonly IHttpContextAccessor? _httpContextAccessor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="httpContextAccessor">HTTP 访问器</param>
        public EcsFormatterConfig(IHttpContextAccessor? httpContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor; // HTTP 访问器
            IncludeHost = true;          // 主机信息
            IncludeProcess = true;       // 进程信息
            IncludeUser = true;          // 用户信息
            IncludeActivityData = true;  // 活动数据（TraceId/SpanId）

            MapCustom = (ecs, logEvent) =>
            {
                ecs.CustomField = "来自业务逻辑";
                return ecs;
            };
        }
    }
}
