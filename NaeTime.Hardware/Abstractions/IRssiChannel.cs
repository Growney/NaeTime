using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Hardware.Abstractions;
public interface IRssiChannel : IAsyncEnumerable<RssiValue>
{
    ValueTask WriteAsync(RssiValue value);
}
