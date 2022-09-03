using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class RssiStreamReadingDto
    {
        public Guid StreamId { get; set; }
        public long Tick { get; set; }
        public int Value { get; set; }
    }
}
