using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Logging.Abstractions
{
    public class LogScope
    {
        public IEnumerable<LogProperty> Properties { get; set; } = new List<LogProperty>();
    }
}
