using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Handlers
{
    public interface IFlightLapHandler
    {
        void HandleStartedLap(Lap lap);
        void HandleCompletedLap(Lap lap);
    }
}
