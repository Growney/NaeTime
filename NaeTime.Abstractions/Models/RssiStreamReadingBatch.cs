using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Models
{
    public class RssiStreamReadingBatch
    {
        public Guid Id { get; set; }
        public Guid RssiStreamId { get; set; }
        public long MinTick { get; set; }
        public long MaxTick { get; set; }
        public int MaxRssiValue { get; set; }
        public int MinRssiValue { get; set; }
        public int ReadingCount { get; set; }
        public DateTime? Processed { get; set; }

        public List<RssiStreamReading> Readings { get; set; } = new();
    }
}
