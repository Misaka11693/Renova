using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Renova.Core.Components.RabbitMQ;

public static class RabbitMQSetup
{
    public static IServiceCollection AddRabbitMQSetup(this IServiceCollection services)
    {
        //注册选项
        services.AddOptions<RabbitMQOptions>()
            .BindConfiguration(RabbitMQOptions.SectionName)
            .ValidateDataAnnotations();

        // 注册RabbitMQ连接
        services.TryAddSingleton<IRabbitMQConnection, RabbitMQConnection>();

        return services;
    }
}
