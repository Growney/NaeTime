using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpio
{
    public static class StopwatchExtensionMethods
    {
        public static void DelayMicroseconds(this Stopwatch stopwatch,int microseconds)
        {
            long numberOfTicksPerSecond = Stopwatch.Frequency;
            long numberOfMicrosecondsPerSecond = 1000000;
            long numberOfTicksPerMicrosecond = numberOfTicksPerSecond / numberOfMicrosecondsPerSecond;

            long numberOfTicksToWait = numberOfTicksPerMicrosecond * microseconds;

            long startingTick = stopwatch.ElapsedTicks;
            while (stopwatch.ElapsedTicks - startingTick < numberOfTicksToWait)
            {
                Thread.SpinWait(1);
            }
        }
    }
}
