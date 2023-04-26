using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Logging.Abstractions
{
    public class LogItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public LogLevel Level { get; set; }
        public LogException Exception { get; set; }
        public string Message { get; set; }
        public IEnumerable<LogProperty> Properties { get; set; } = new List<LogProperty>();
    }
}
