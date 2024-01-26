using NaeTime.Timing.Abstractions.Models;

namespace NaeTime.Timing.Abstractions;
public interface ITimer
{
    Task InitialiseAsync(CancellationToken token);
    Task<Detection?> WaitForNextDetectionAsync(CancellationToken token);
}
