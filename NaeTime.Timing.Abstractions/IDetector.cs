using NaeTime.Timing.Abstractions.Models;

namespace NaeTime.Timing.Abstractions;
public interface IDetector
{
    Task<Detection?> WaitForNextDetectionAsync(CancellationToken token);
}
