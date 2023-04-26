using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Logging.Abstractions;

namespace Core.Logging.Core
{
    public class CalculatedLogPropertyProvider : IGlobalLogPropertyProvider
    {
        private readonly long _flag;
        private readonly Func<KeyValuePair<string, object>> _func;

        public CalculatedLogPropertyProvider(Func<KeyValuePair<string, object>> func)
            : this(LogLevel.Trace, LogLevel.Critical, func)
        {
        }
        public CalculatedLogPropertyProvider(LogLevel minimuim, LogLevel maximum, Func<KeyValuePair<string, object>> func)
        {
            _flag = minimuim.GetFlagValue(maximum);
            _func = func;
        }

        public IEnumerable<KeyValuePair<string, object>> GetProperties(LogLevel level)
        {
            if (level.IncludedInFlag(_flag))
            {
                yield return _func();
            }
        }
    }
}
