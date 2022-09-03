using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Abstractions.Models
{
    public class FlightSplitEventArgs : EventArgs
    {
        public FlightSplitEventArgs(SplitDto split)
        {
            Split = split ?? throw new ArgumentNullException(nameof(split));
        }

        public SplitDto Split { get; }
    }
}
