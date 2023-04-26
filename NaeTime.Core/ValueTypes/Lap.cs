using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Core.ValueTypes
{
    public class Lap
    {
        public long StartingTick { get; set; }
        public long? EndingTick { get; set; }

        public Lap(long startingTick)
        {
            StartingTick = startingTick;
        }
    }
}
