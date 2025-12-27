using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renova.Core.Common.Extensions;
using Serilog;
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

        // 控制器名称
        var controllerName = actionMethod.DeclaringType?.Name ?? "UnknownController";

        // 操作名称
        var actionName = actionMethod.Name;

        // 显示名称
        var displayName = actionMethod.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? string.Empty;

        // http 请求方式
        var httpRequestMethod = httpContext.Request.Method;

        // http 请求 url
        // $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        var httpRequestUrl = httpContext.Request.GetDisplayUrl();

        // http 协议
        var httpProtocol = httpContext.Request.Protocol;

        // http 来源地址
        var httpReferer = httpContext.Request.Headers["Referer"].ToString();

        // http 请求端源（swagger还是其他）
        var httpUserAgent = httpContext.Request.Headers["request-from"].ToString() ?? "client";

        // 浏览器标识
        var httpBrowserAgent = httpContext.Request.Headers["User-Agent"].ToString();

        // 客户端区域语言
        var httpAcceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();

        // 客户端 IP 地址
        var httpClientIp = GetClientIpAddress(httpContext.Connection.RemoteIpAddress);

        // 客户端源端口
        var httpClientPort = httpContext.Connection.RemotePort;

        // 服务端 IP 地址
        var httpServerIp = GetClientIpAddress(httpContext.Connection.LocalIpAddress);

        // 服务端源端口
        var httpServerPort = httpContext.Connection.LocalPort;

        // 客户端连接 ID
        var httpConnectionId = httpContext.Connection.Id;

        // 服务线程 ID
        var httpThreadId = Environment.CurrentManagedThreadId;

        // 系统名称
        var osDescription = RuntimeInformation.OSDescription;

        // 系统架构
        var osArchitecture = RuntimeInformation.OSArchitecture.ToString();

        // .NET 架构
        var frameworkArchitecture = RuntimeInformation.FrameworkDescription;

        // Web 启动地址（监听地址，非客户端访问地址）
        var processStartUrl = httpContext.RequestServices.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;

        // 运行环境
        var environmentName = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName;

        // 启动程序集（在 Web 托管环境下 GetEntryAssembly() 可能为 null）
        var entryAssembly = Assembly.GetEntryAssembly()?.GetName().Name ?? httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ApplicationName;

        // 托管程序
        var hostingProcess = GetHostingProcessType(entryAssembly);

        var result = await next();

        var logItems = new[]
        {
            $"##控制器名称## {controllerName}",
            $"##控制器名称## {controllerName}",
            $"##操作方法## {actionName}",
            $"##显示名称## {displayName}",
            "",
            $"##请求方式## {httpRequestMethod}",
            $"##请求地址## {httpRequestUrl}",
            $"##HTTP 协议## {httpProtocol}",
            $"##来源页面## {httpReferer}",
            "",
            $"##客户端地址## {httpClientIp}:{httpClientPort}",
            $"##服务端地址## {httpServerIp}:{httpServerPort}",
            $"##连接标识## {httpConnectionId}",
            "",
            $"##请求来源## {httpUserAgent}",
            $"##浏览器标识## {httpBrowserAgent}",
            $"##接受语言## {httpAcceptLanguage}",
            "",
            $"##托管程序## {hostingProcess}",
            $"##运行环境## {environmentName}",
            $"##入口程序集## {entryAssembly}",
            $"##监听地址## {string.Join("，", processStartUrl ?? ["N/A"])}",
            "",
            $"##操作系统## {osDescription}",
            $"##系统架构## {osArchitecture}",
            $"##.NET 版本## {frameworkArchitecture}",
            $"##线程标识## {httpThreadId}"
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

        sb.AppendLine($"┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ {title} ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
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
}

/// <summary>
/// LoggingMonitor 日志拓展默认分类名
/// </summary>
internal sealed class LoggingMonitor { }
