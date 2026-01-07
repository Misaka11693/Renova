using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewLife.Http;
using Renova.Core.Common.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Renova.Core;

/// <summary>
/// 日志监听特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class LoggingMonitorAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// 过滤器执行顺序：靠前执行，在其他业务过滤器之前打印日志
    /// </summary>
    public int Order => -2000;

    /// <summary>
    /// 日志标题
    /// </summary>
    public string Title { get; set; } = "日志监控";

    /// <summary>
    /// 模板正则（匹配 ##字段## 值）
    /// </summary>
    private static readonly Regex _propRegex = new(@"^##(?<prop>.+?)##\s*(?<content>.*)", RegexOptions.Compiled);

    /// <summary>
    /// 拦截 Web API Action 执行
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionMethod = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
        if (actionMethod == null)
        {
            await next();
            return;
        }

        await ExecuteLoggingMonitorAsync(actionMethod, context.ActionArguments, context, next);
    }

    /// <summary>
    /// 执行监控
    /// </summary>
    private async Task ExecuteLoggingMonitorAsync(MethodInfo actionMethod, IDictionary<string, object?> parameterValues, FilterContext context, ActionExecutionDelegate next)
    {
        // WebSocket 跳过
        if (context.HttpContext.IsWebSocketRequest())
        {
            _ = await next();
            return;
        }

        // http 上下文
        var httpContext = context.HttpContext;

        // 终结点显示名称
        var endpointDisplayName = httpContext.GetEndpoint()?.DisplayName;

        // 控制器与方法信息
        var controllerName = actionMethod.DeclaringType?.Name;
        var actionName = actionMethod.Name;
        var displayName = actionMethod.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;

        // HTTP 信息
        var httpRequestMethod = httpContext.Request.Method;
        var httpRequestUrl = httpContext.Request.GetDisplayUrl();
        var httpProtocol = httpContext.Request.Protocol;
        var httpReferer = httpContext.Request.Headers["Referer"].ToString();

        // 客户端来源
        var httpRequestFrom = httpContext.Request.Headers["request-from"].ToString();
        httpRequestFrom = string.IsNullOrWhiteSpace(httpRequestFrom) ? "client" : httpRequestFrom;
        var httpBrowserAgent = httpContext.Request.Headers["User-Agent"].ToString();
        var httpAcceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();

        // 网络连接信息
        var httpClientIp = GetClientIpAddress(httpContext.Connection.RemoteIpAddress);
        var httpClientPort = httpContext.Connection.RemotePort;
        var httpServerIp = GetClientIpAddress(httpContext.Connection.LocalIpAddress);
        var httpServerPort = httpContext.Connection.LocalPort;
        var httpConnectionId = httpContext.Connection.Id;

        // 线程与系统信息
        var machineName = System.Environment.MachineName;
        var httpThreadId = Environment.CurrentManagedThreadId;
        var osDescription = RuntimeInformation.OSDescription;
        var osArchitecture = RuntimeInformation.OSArchitecture.ToString();
        var frameworkArchitecture = RuntimeInformation.FrameworkDescription;

        // 服务启动信息
        var processStartUrl = httpContext.RequestServices.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
        var environmentName = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName;
        var entryAssembly = Assembly.GetEntryAssembly()?.GetName().Name ?? httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ApplicationName;
        var hostingProcess = GetHostingProcessType(entryAssembly);

        // 授权信息
        bool isAuthenticated = httpContext!.User!.Identity!.IsAuthenticated == true;
        string authenticationStatus = isAuthenticated ? "已认证" : "未认证";
        var accessToken = NormalizeLogValue(httpContext.Request.Headers["Authorization"].ToString());
        var refreshToken = NormalizeLogValue(httpContext.Request.Headers["refresh_token"].ToString());
        string userName = isAuthenticated ? (httpContext.User.FindFirst(ClaimConst.UserId)?.Value ?? "未知用户") : "匿名用户";


        // 记录开始时间 & 启动计时器
        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        // 执行 Action
        var result = await next();

        // 停止计时
        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        // 获取异常信息
        var exception = result.Exception;
        bool hasException = exception != null;
        string exceptionStatus = hasException ? "已发生" : "无异常";
        string exceptionType = NormalizeLogValue(hasException ? exception!.GetType().FullName : null);
        string exceptionMessage = NormalizeLogValue(hasException ? exception!.Message : null);
        string exceptionSource = NormalizeLogValue(hasException ? exception!.Source : null);
        string exceptionStackTrace = NormalizeLogValue(hasException ? exception!.StackTrace : null);


        var logItems = new string[]
        {
            "",
            $"{endpointDisplayName}",
            "",
            "━━━━━━━━━━━━━━━  路由信息 ━━━━━━━━━━━━━━━",
            $"##控制器名称## {controllerName}",
            $"##操作方法## {actionName}",
            $"##显示名称## {displayName}",
            "",
            "━━━━━━━━━━━━━━━  HTTP 请求 ━━━━━━━━━━━━━━━",
            $"##请求方式## {httpRequestMethod}",
            $"##请求地址## {httpRequestUrl}",
            $"##HTTP 协议## {httpProtocol}",
            $"##来源页面## {httpReferer}",
            "",
            "━━━━━━━━━━━━━━━  连接信息 ━━━━━━━━━━━━━━━",
            $"##客户端地址## {httpClientIp}:{httpClientPort}",
            $"##服务端地址## {httpServerIp}:{httpServerPort}",
            $"##连接标识## {httpConnectionId}",
            "",
            "━━━━━━━━━━━━━━━  客户端上下文 ━━━━━━━━━━━━━━━",
            $"##请求来源## {httpRequestFrom}",
            $"##浏览器标识## {httpBrowserAgent}",
            $"##接受语言## {httpAcceptLanguage}",
            "",
            "━━━━━━━━━━━━━━━  服务端环境 ━━━━━━━━━━━━━━━",
            $"##托管程序## {hostingProcess}",
            $"##运行环境## {environmentName}",
            $"##入口程序集## {entryAssembly}",
            $"##监听地址## {string.Join("，", processStartUrl ?? ["N/A"])}",
            "",
            "━━━━━━━━━━━━━━━  执行与系统 ━━━━━━━━━━━━━━━",
            $"##开始时间## {startTime:yyyy-MM-dd HH:mm:ss.fff}",
            $"##执行耗时## {elapsedMilliseconds} ms",
            $"##线程标识## {httpThreadId}",
            $"##服务器名称## {machineName}",
            $"##操作系统## {osDescription}",
            $"##系统架构## {osArchitecture}",
            $"##.NET 版本## {frameworkArchitecture}",
            "",
            $"━━━━━━━━━━━━━━━  授权信息 ━━━━━━━━━━━━━━━",
            $"##身份认证## {authenticationStatus}",
            $"##用户标识## {userName}",
            $"##访问令牌## {accessToken}",
            $"##刷新令牌## {refreshToken}",
            "",
            $"━━━━━━━━━━━━━━━  异常信息 ━━━━━━━━━━━━━━━",
            $"##异常状态## {exceptionStatus}",
            $"##异常类型## {exceptionType}",
            $"##异常消息## {exceptionMessage}",
            $"##异常来源## {exceptionSource}",
            $"##异常堆栈## {exceptionStackTrace}",
            "",
        };

        var logContent = BuildAlignedLog(Title, logItems);
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<LoggingMonitor>>();
        logger.LogInformation("\n{LogContent}", logContent);
    }

    /// <summary>
    /// 获取客户端 IP 地址
    /// </summary>
    /// <param name="address">客户端的 IP 地址</param>
    private static string GetClientIpAddress(System.Net.IPAddress? address)
    {
        if (address == null)
            return "Unknown";

        // 如果是 IPv4 映射的 IPv6 地址（如 ::ffff:192.168.1.1），转为标准 IPv4
        if (address.IsIPv4MappedToIPv6)
            return address.MapToIPv4().ToString();

        return address.ToString();
    }

    /// <summary>
    /// 获取应用的托管方式
    /// </summary>
    /// <param name="entryAssemblyName">应用入口程序集名称</param>
    private static string GetHostingProcessType(string entryAssemblyName)
    {
        var processName = Process.GetCurrentProcess().ProcessName;
        var isIISVersionSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_IIS_VERSION"));

        if (processName.Equals("w3wp", StringComparison.OrdinalIgnoreCase))
            return "IIS";

        if (processName.Equals("iisexpress", StringComparison.OrdinalIgnoreCase))
            return "IIS Express";

        if (isIISVersionSet)
            return "IIS (via ANCM)";

        if (!string.IsNullOrEmpty(entryAssemblyName) &&
            (processName.Equals(entryAssemblyName, StringComparison.OrdinalIgnoreCase) ||
             processName.Equals(entryAssemblyName.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)))
            return "Kestrel (Self-Hosted)";

        if (processName.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            return "Kestrel (via dotnet CLI)";

        return $"Kestrel ({processName})";
    }

    /// <summary>
    /// 生成对齐日志
    /// </summary>
    private static string BuildAlignedLog(string title, string[] items)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ {title} ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        if (items != null && items.Length > 0)
        {
            // 找出所有 ##字段## 中“字段”部分的最大显示宽度
            int maxPropWidth = 0;
            foreach (var item in items)
            {
                if (_propRegex.IsMatch(item))
                {
                    var prop = _propRegex.Match(item).Groups["prop"].Value;
                    var width = GetDisplayWidth(prop + "："); // 加冒号
                    if (width > maxPropWidth) maxPropWidth = width;
                }
            }

            // 保证最小宽度
            if (maxPropWidth < 12) maxPropWidth = 12;
            maxPropWidth += 2; // 额外留白

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item))
                {
                    sb.AppendLine("┣");
                    continue;
                }

                if (_propRegex.IsMatch(item))
                {
                    var match = _propRegex.Match(item);
                    var prop = match.Groups["prop"].Value;
                    var content = match.Groups["content"].Value;
                    var label = prop + "：";
                    var paddedLabel = PadRightForConsole(label, maxPropWidth);
                    sb.AppendLine($"┣ {paddedLabel} {content}");
                }
                else
                {
                    sb.AppendLine($"┣ {item}");
                }
            }
        }

        sb.AppendLine($"┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        return sb.ToString();
    }

    /// <summary>
    /// 计算字符串在终端中的显示宽度（中文=2，英文=1）
    /// </summary>
    private static int GetDisplayWidth(string str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        var width = 0;
        foreach (char c in str)
        {
            width += char.IsAscii(c) && c >= 32 ? 1 : 2;
        }
        return width;
    }

    /// <summary>
    /// 按显示宽度右补齐（用于标签对齐）
    /// </summary>
    private static string PadRightForConsole(string text, int totalWidth)
    {
        var currentWidth = GetDisplayWidth(text);
        var spaces = Math.Max(0, totalWidth - currentWidth);
        return text + new string(' ', spaces);
    }

    /// <summary>
    /// 规范化日志字段值：空值统一输出 "-"
    /// </summary>
    private static string NormalizeLogValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}

/// <summary>
/// LoggingMonitor 日志拓展默认分类名
/// </summary>
internal sealed class LoggingMonitor { }
