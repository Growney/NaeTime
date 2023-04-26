using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Logging.Abstractions
{
    public static class LogLevelExtensionMethods
    {
        public static long GetFlagValue(this LogLevel min, LogLevel max)
        {
            int minInt = (int)min;
            int maxInt = (int)max;
            if (minInt > maxInt)
            {
                int temp = maxInt;
                maxInt = minInt;
                minInt = temp;
            }
            long value = (long)Math.Pow(2, minInt);

            for (int i = minInt + 1; i <= maxInt; i++)
            {
                value |= (long)Math.Pow(2, i);
            }

            return value;
        }

        public static bool IncludedInFlag(this LogLevel level, long flag)
        {
            long levelValue = (long)level;
            return (flag & levelValue) == levelValue;
        }
    }
}
