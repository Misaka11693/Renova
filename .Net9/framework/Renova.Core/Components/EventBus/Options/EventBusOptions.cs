using Renova.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core.Components.EventBus.Options;

public class EventBusOptions : IConfigSectionProvider
{
    public static string SectionName => "EventBusOptions";

    /// <summary>
    /// 事件总线存储类型，默认是内存。可选：Memory、RabbitMQ
    /// </summary>
    public string StorageType { get; set; } = "Memory";
}
