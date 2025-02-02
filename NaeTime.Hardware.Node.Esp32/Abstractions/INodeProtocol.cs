namespace NaeTime.Hardware.Node.Esp32.Abstractions;

public interface INodeProtocol
{
    INodeTimingProtocol TimingProtocol { get; }
    INodeConfigurationProtocol ConfigurationProtocol { get; }
    Task RunAsync(CancellationToken token);
}