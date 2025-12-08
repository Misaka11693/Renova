using Microsoft.Extensions.DependencyInjection;
using Renova.Core.Apps;
using Renova.Core.Components.EventBus.Internal;
using Renova.Core.Components.EventBus.Options;
using Renova.Core.Components.EventBus.Storers;
using Renova.Core.Components.RabbitMQ;

namespace Renova.Core.Components.EventBus;

public static class EventBusSetup
{
    public static IServiceCollection AddEventBusSetup(this IServiceCollection services)
    {
        //注册选项
        services.AddOptions<EventBusOptions>()
            .BindConfiguration(EventBusOptions.SectionName)
            .ValidateDataAnnotations();

        //获取配置
        var options = App.GetOptions<EventBusOptions>();

        if (string.Equals(options.StorageType, "Memory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddEventBus();
        }
        else if (string.Equals(options.StorageType, "RabbitMQ", StringComparison.OrdinalIgnoreCase))
        {
            services.AddRabbitMQSetup();

            services.AddEventBus(options =>
            {
                var rbmqEventSourceStorer = ActivatorUtilities.CreateInstance<RabbitMQEventSourceStorer>(services.BuildServiceProvider(), "eventbus", 3000);

                options.ReplaceStorer(serviceProvider =>
                {
                    return rbmqEventSourceStorer;
                });

                options.ReplacePublisher<DistributedEventPublisher>();
            });
        }
        else
        {
            throw new Exception($"不支持的存储类型：{options.StorageType}");
        }

        return services;
    }
}
