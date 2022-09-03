using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Processors
{
    public interface IRssiStreamReadingProcessor : IFlightProcessor
    {
        void ProcessReading(RssiStreamReading reading);

        IEnumerable<RssiStreamReading> GetGeneratedReadings();
        IEnumerable<RssiStreamPass> GetStartedPasses();
        IEnumerable<RssiStreamPass> GetCompletedPasses();
    }
}
