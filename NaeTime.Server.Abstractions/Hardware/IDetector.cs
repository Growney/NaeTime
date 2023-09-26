using NaeTime.Server.Abstractions.Models;

namespace NaeTime.Server.Abstractions.Hardware;
public interface IDetector
{
    Task<Detection> WaitForNextDetectionAsync(CancellationToken cancellationToken);
}
