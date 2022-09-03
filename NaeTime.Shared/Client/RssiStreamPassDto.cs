using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class RssiStreamPassDto
    {
        public Guid Id { get; set; }
        public Guid RssiStreamId { get; set; }
        public long Start { get; set; }
        public long? End { get; set; }
    }
}
