using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Logging.Abstractions
{
    public interface ILogQueue
    {
        void Enqueue(LogEntry item);
        bool TryDequeue(out LogEntry item);
    }
}
