using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Handlers
{
    public interface IRssiStreamReadingHandler
    {
        void HandleReading(RssiStreamReading reading);
    }
}
