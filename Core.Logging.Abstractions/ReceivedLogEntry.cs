using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Logging.Abstractions
{
    public class ReceivedLogEntry : LogEntry
    {
        public DateTime ReceivedDateTime { get; set; }
    }
}
