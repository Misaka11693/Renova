using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Renova.Core;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Renova.Swagger;

public static class SwaggerAppExtensions
{
    /// <summary>
    /// 配置Swagger中间件
    /// </summary>
    public static IApplicationBuilder UseSwaggerSetup(this IApplicationBuilder app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            #region 动态加载分组
            // 1. 预注册分组
            foreach (var doc in SwaggerConstants.ApiDocs)
            {
                c.SwaggerEndpoint($"/swagger/{doc.Key}/swagger.json", $"{doc.Value.Title}");
            }

            // 2. 动态加载其他分组（排除预注册分组）
            var provider = App.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            provider.ApiDescriptionGroups.Items
                .Where(g =>
                    !string.IsNullOrEmpty(g.GroupName) &&
                    !SwaggerConstants.ApiDocs.ContainsKey(g.GroupName))
                .OrderBy(g => g.GroupName)
                .ToList()
                .ForEach(group =>
                    c.SwaggerEndpoint(
                        $"/swagger/{group.GroupName}/swagger.json",
                        $"{group.GroupName}"
                    )
                );
            #endregion

            #region UI配置
            //c.RoutePrefix = "swagger";
            c.RoutePrefix = string.Empty; // 设置Swagger UI为默认首页路径
            c.DocExpansion(DocExpansion.List);//折叠所有标签
            c.DefaultModelsExpandDepth(-1); // 隐藏所有模型
            c.DisplayRequestDuration(); // 显示请求持续时间 
            #endregion
        });

        return app;
    }
}
