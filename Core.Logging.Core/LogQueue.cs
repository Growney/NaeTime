using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Core.Logging.Abstractions;

namespace Core.Logging.Core
{
    public class LogQueue : ConcurrentQueue<LogEntry>, ILogQueue
    {
    }
}
