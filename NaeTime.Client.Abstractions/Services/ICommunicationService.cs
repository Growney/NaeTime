using NaeTime.Client.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Abstractions.Services
{
    public interface ICommunicationService
    {
        event EventHandler<FlightLapDtoEventArgs> OnLapStarted;
        event EventHandler<FlightLapDtoEventArgs> OnLapCompleted;

        event EventHandler<FlightSplitEventArgs> OnSplitStarted; 
        event EventHandler<FlightSplitEventArgs> OnSplitCompleted;

        event EventHandler<RssiStreamReadingEventArgs> OnRssiStreamReading;

        Task StartAsync();
        Task StopAsync();
    }
}
