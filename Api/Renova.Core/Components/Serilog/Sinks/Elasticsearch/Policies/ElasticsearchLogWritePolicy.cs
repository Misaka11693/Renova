using Renova.Core.Components.Serilog.Abstractions;
using Serilog.Events;

namespace Renova.Core.Components.Serilog.Sinks.Elasticsearch.Policies
{
    public class ElasticsearchLogWritePolicy : IElasticsearchLogWritePolicy
    {
        public bool ShouldWrite(LogEvent e)
        {
            if (e.Exception != null)
                return true;

            if (e.Level >= LogEventLevel.Error)
                return true;

            if (e.Properties.ContainsKey("RequestPath"))
                return true;

            return false;
        }
    }
}
