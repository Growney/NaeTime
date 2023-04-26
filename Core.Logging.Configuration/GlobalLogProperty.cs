using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Logging.Configuration
{
    public class GlobalLogProperty
    {
        public LogLevel Minimum { get; set; } = LogLevel.Trace;
        public LogLevel Maximum { get; set; } = LogLevel.Critical;

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
