using NaeTime.Node.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface IRssiReceiver
    {
        Guid Id { get; }
        int AssignedFrequency { get; }
        int TunedFrequency { get; }
        Task<bool> TuneAsync(int frequencyInMhz);

        RssiStream? CurrentStream { get; }
        int GetRssi();

    }
}
