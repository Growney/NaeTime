using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Logging.Abstractions;

namespace Core.Logging.Core
{
    public class StaticLogPropertyProvider : IGlobalLogPropertyProvider
    {
        private readonly long _flag;
        private readonly string _key;
        private readonly object _value;

        public StaticLogPropertyProvider(string key, object value)
            : this(LogLevel.Trace, LogLevel.Critical, key, value)
        {
        }
        public StaticLogPropertyProvider(LogLevel minimuim, LogLevel maximum, string key, object value)
        {
            _flag = minimuim.GetFlagValue(maximum);
            _key = key;
            _value = value;
        }
        public IEnumerable<KeyValuePair<string, object>> GetProperties(LogLevel level)
        {
            if (level.IncludedInFlag(_flag))
            {
                yield return new KeyValuePair<string, object>(_key, _value);
            }
        }
    }
}
