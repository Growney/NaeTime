namespace NaeTime.Hardware.Node.Esp32.Abstractions;
public interface INodeTimingProtocol : INodeSubProtocol
{
    Task<ReceivedSignalStrengthIndicator?> WaitForNextReceivedSignalStrengthIndicatorAsync(CancellationToken cancellationToken);
    Task<Pass> WaitForNextPassAsync(CancellationToken cancellationToken);
}
