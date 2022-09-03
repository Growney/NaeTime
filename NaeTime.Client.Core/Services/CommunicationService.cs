using NaeTime.Client.Abstractions.Models;
using NaeTime.Client.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Core.Services
{
    public class CommunicationService : ICommunicationService
    {
        public event EventHandler<FlightLapDtoEventArgs>? OnLapStarted;
        public event EventHandler<FlightLapDtoEventArgs>? OnLapCompleted;
        public event EventHandler<FlightSplitEventArgs>? OnSplitStarted;
        public event EventHandler<FlightSplitEventArgs>? OnSplitCompleted;
        public event EventHandler<RssiStreamReadingEventArgs>? OnRssiStreamReading;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
