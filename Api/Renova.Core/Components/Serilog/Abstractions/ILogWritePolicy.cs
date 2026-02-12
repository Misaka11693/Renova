using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renova.Core.Components.Serilog.Abstractions
{
    public interface ILogWritePolicy
    {
        bool ShouldWrite(LogEvent logEvent);
    }
}
