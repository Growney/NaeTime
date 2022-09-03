using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Shared.Client
{
    public class RssiBoundaryDto
    {
        public int Id { get; set; }
        public int PeakStartRssi { get; set; }
        public int PeakEndRssi { get; set; }
    }
}
