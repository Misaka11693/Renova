using System;
using System.Collections.Generic;
using System.Text;

namespace Renova.Core.Components.Serilog.Options;

public sealed class SerilogOptions
{
    public bool EnableElk { get; set; } = false;

    public string? ElasticsearchUrl { get; set; } = null;

    public string IndexPrefix { get; set; } = "renova-admin";

    /// <summary>
    /// 启用 ELK 时是否仍写文件
    /// </summary>
    public bool WriteFileWhenElkEnabled { get; set; } = true;
}
