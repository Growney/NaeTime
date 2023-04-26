using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Logging.Core
{
    public interface IGlobalLogPropertyProvider
    {
        IEnumerable<KeyValuePair<string, object>> GetProperties(LogLevel level);
    }
}
