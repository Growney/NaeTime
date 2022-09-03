using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Node
{
    public class RssiStreamValuesDto
    {
        public Guid StreamId { get; set; }
        public List<RssiValueDto>? RssiValues { get; set; }
    }
}
