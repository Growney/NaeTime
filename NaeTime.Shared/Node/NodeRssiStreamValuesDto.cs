using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Node
{
    public class NodeRssiStreamValuesDto
    {
        public Guid NodeId { get; set; }
        public List<RssiStreamValuesDto>? Values { get; set; }
    }
}
