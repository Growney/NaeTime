using NaeTime.Server.Abstractions.Hardware;
using NaeTime.Server.Abstractions.Models;

namespace NaeTime.Server.LapRF.Hardware;
public class LapRFDetector : IDetector
{
    public Task<Detection> WaitForNextDetectionAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
