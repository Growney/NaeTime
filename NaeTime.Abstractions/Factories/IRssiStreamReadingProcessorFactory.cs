using NaeTime.Abstractions.Processors;

namespace NaeTime.Abstractions.Factories
{
    public interface IRssiStreamReadingProcessorFactory
    {
        Task<IRssiStreamReadingProcessor> CreateProcessorAsync(Guid streamId);

        Task SaveChangedAsync();
    }
}
