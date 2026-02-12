using Elastic.CommonSchema.Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Renova.Core.Components.Serilog.Sinks.Elasticsearch.Ecs
{
    public class MyEcsDocument : LogEventEcsDocument
    {
        [JsonPropertyName("custom.field")]
        public string? CustomField { get; set; }
    }
}
