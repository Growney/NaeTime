using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Abstractions.Domain;

public interface ITuningCommunication
{
    int TunedFrequency { get; }
    int ActualFrequency { get; }
    Task<TuningResult> TuneDeviceAsync(int frequency);
}
