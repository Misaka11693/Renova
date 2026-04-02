using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renova.Core.Components.Modular;

/// <summary>
/// 模块化应用模块接口，解耦
/// </summary>
public interface IAppModule
{
    /// <summary>
    /// 配置服务
    /// </summary>
    void ConfigServices(IServiceCollection services) { }


    /// <summary>
    /// 配置app应用构建器
    /// </summary>
    void ConfigureApp(IApplicationBuilder app) { }
}
