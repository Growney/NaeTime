using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Abstractions.Domain;

public interface INodeDeviceFactory
{
    Task<NodeDevices> GetNodeDevices(NodeConfiguration nodeConfiguration);
}
