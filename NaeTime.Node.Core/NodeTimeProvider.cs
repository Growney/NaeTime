using NaeTime.Node.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Core
{
    public class NodeTimeProvider : INodeTimeProvider
    {
        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public NodeTimeProvider()
        {
            _stopwatch.Start();
        }
    }
}
