using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Logging.Abstractions
{
    public class LogException
    {
        public LogException InnerException { get; set; }

        public string Message { get; set; }
        public string Type { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
    }
}
