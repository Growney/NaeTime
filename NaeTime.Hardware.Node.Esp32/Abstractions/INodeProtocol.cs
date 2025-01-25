namespace NaeTime.Hardware.Node.Esp32.Abstractions;

public interface INodeProtocol
{
    Task RunAsync(CancellationToken token);
}