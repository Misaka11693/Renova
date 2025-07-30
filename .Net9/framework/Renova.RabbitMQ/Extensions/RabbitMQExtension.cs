using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Renova.RabbitMQ.Interfaces;
using Renova.RabbitMQ.Options;

namespace Renova.RabbitMQ;

public static class RabbitMQExtension
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
