using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Abstractions.Models
{
    public class RssiStreamReadingEventArgs : EventArgs
    {
        public RssiStreamReadingEventArgs(RssiReadingDto reading)
        {
            Reading = reading ?? throw new ArgumentNullException(nameof(reading));
        }

        public RssiReadingDto Reading { get; }
    }
}
