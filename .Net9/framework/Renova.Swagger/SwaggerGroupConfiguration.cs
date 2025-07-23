using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Renova.Swagger;

public class SwaggerGroup : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiDescriptionGroupCollectionProvider _provider;

    public SwaggerGroup(IApiDescriptionGroupCollectionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // 注册特殊分组
        RegisterRequiredGroups(options);

        // 动态注册特性分组
        RegisterAttributeGroups(options);

        // 分组规则
        ConfigureInclusionRules(options);

        // 配置操作排序
        options.OrderActionsBy(apiDesc => apiDesc.GroupName);
    }

    private void RegisterRequiredGroups(SwaggerGenOptions options)
    {
        foreach (var doc in SwaggerSetup.ApiDocs)
        {
            options.SwaggerDoc(doc.Key, doc.Value);
        }
    }

    private void RegisterAttributeGroups(SwaggerGenOptions options)
    {
        // 获取所有接口的分组信息
        var groupNames = _provider.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .Select(api => api.GroupName)
            .Where(groupName =>
                !string.IsNullOrEmpty(groupName) &&  // 过滤未分组
                !SwaggerSetup.ApiDocs.ContainsKey(groupName))// 排除
            .Distinct();

        // 动态注册特性分组
        foreach (var name in groupNames)
        {
            options.SwaggerDoc(name, new OpenApiInfo
            {
                Title = $"{name} 接口",
                Version = "v1.0.0",
                Description = "生命如同一场旅程，每一步都值得珍惜；无论风雨还是晴空，未来总会因你的努力而更加美好"
            });
        }
    }

    private void ConfigureInclusionRules(SwaggerGenOptions options)
    {
        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            // 规则1：All分组包含所有接口
            if (docName == SwaggerSetup.AllGroupName) return true;

            // 规则2：获取当前接口的分组名称（由[ApiExplorerSettings]设置）
            var groupName = apiDesc.GroupName;

            // 规则3：Default分组的特殊处理
            if (docName == SwaggerSetup.DefaultGroupName)
                return string.IsNullOrEmpty(groupName) || groupName == SwaggerSetup.DefaultGroupName;

            // 规则4：其他分组进行名称匹配
            return groupName == docName;
        });
    }
}