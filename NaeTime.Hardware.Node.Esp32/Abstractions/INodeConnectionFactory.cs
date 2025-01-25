namespace NaeTime.Hardware.Node.Esp32.Abstractions;

internal interface INodeConnectionFactory
{
    NodeConnection CreateSerialConnection(Guid timerId, string port);
}