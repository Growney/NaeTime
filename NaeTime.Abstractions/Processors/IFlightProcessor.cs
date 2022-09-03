using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Processors
{
    public interface IFlightProcessor
    {
        IEnumerable<Lap> GetStartedLaps();
        IEnumerable<Lap> GetCompletedLaps();
        IEnumerable<Split> GetStartedSplits();
        IEnumerable<Split> GetCompletedSplits();
    }
}
