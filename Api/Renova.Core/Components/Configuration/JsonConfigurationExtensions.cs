using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Components.Configuration.Options;

namespace Renova.Core.Components.Configuration;

/// <summary>
/// Json 配置加载
/// </summary>
public static class JsonConfigurationExtensions
{
    /// <summary>
    /// 加载动态 JSON 配置文件
    /// 注意配置优先级（appsettings.json 最低）
    /// </summary>
    /// <param name="builder">WebApplicationBuilder 实例</param>
    /// <returns>WebApplicationBuilder 实例</returns>
    public static WebApplicationBuilder AddDynamicJsonFiles(this WebApplicationBuilder builder)
    {
        // 绑定配置选项
        builder.Services.AddOptions<JsonConfigOptions>()
            .BindConfiguration(JsonConfigOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(options =>
            {
                // 自定义验证：Include 数组必须全部以 .json 结尾
                return options.Include != null && options.Include.All(p => p.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
            }, "JsonConfigOptions.Include 中的文件模式必须全部以 .json 结尾")
            .ValidateOnStart();

        //// 获取配置选项
        //var options = new JsonConfigOptions(); // 此时已有默认值
        //builder.Configuration.GetSection(JsonConfigOptions.SectionName).Bind(options);

        // 获取配置选项
        var options = builder.Configuration.GetSection(JsonConfigOptions.SectionName).Get<JsonConfigOptions>();

        if (options == null || !options.Enabled || string.IsNullOrEmpty(options.Folder))
            return builder;

        var env = builder.Environment.EnvironmentName;

        // 只扫描 AppContext.BaseDirectory 下的指定文件夹即可,因为被引用的类库，他们的JSON配置文件会被复制到主程序目录下，无需再去扫描类库目录
        var basePath = AppContext.BaseDirectory;
        var folderPath = Path.Combine(basePath, options.Folder);
        if (!Directory.Exists(folderPath))
            return builder;

        // 如果没有指定 Include，代表不加载任何文件
        if (options.Include == null || options.Include.Length == 0)
            return builder;

        // 找到符合 Include / Exclude 的 JSON 文件
        var files = options.Include.Distinct()
            .SelectMany(p => Directory.GetFiles(folderPath, p, SearchOption.TopDirectoryOnly))
            .Where(f => !options.Exclude.Distinct().Contains(Path.GetFileName(f), StringComparer.OrdinalIgnoreCase))
            .ToList();


        // 加载基础 JSON
        foreach (var file in files.Where(f => !IsEnvironmentFile(f, env)))
        {
            builder.Configuration.AddJsonFile(file, optional: true, reloadOnChange: true);
        }

        // 加载环境 JSON 覆盖
        foreach (var file in files.Where(f => IsEnvironmentFile(f, env)))
        {
            builder.Configuration.AddJsonFile(file, optional: true, reloadOnChange: true);
        }

        // 最后加载环境变量覆盖
        builder.Configuration.AddEnvironmentVariables();

        return builder;
    }

    /// <summary>
    /// 判断文件是否为环境配置文件
    /// </summary>
    private static bool IsEnvironmentFile(string file, string env)
    {
        return file.EndsWith($".{env}.json", StringComparison.OrdinalIgnoreCase);
    }
}
