using Renova.Core.Components.Serilog.Abstractions;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renova.Core.Components.Serilog.Sinks.Database.Policies
{
    public class DatabaseLogWritePolicy : IDatabaseLogWritePolicy
    {
        public bool ShouldWrite(LogEvent e)
        {
            if (e.Level < LogEventLevel.Warning)
                return false;

            if (e.Properties.ContainsKey("BusinessCode"))
                return true;

            return false;
        }
    }
}
