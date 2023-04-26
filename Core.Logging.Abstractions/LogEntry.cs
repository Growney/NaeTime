using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Logging.Abstractions
{
    public class LogEntry
    {
        public DateTime DateTime { get; set; }
        public IEnumerable<LogScope> Scopes { get; set; } = new List<LogScope>();
        public LogItem Item { get; set; }
    }
}
