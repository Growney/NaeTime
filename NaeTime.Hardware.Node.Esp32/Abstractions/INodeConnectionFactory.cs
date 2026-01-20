using System.Net;

namespace NaeTime.Hardware.Node.Esp32.Abstractions;

internal interface INodeConnectionFactory
{
    NodeConnection CreateSerialConnection(Guid timerId, string port);
    NodeConnection CreateTcpConnection(Guid timerId, IPAddress ipAddress, ushort port);
}