using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface INodeTimeProvider
    {
        long ElapsedMilliseconds { get; }
    }
}
