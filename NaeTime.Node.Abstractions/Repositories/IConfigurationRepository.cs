using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Abstractions.Repositories;

public interface IConfigurationRepository
{
    Task<NodeConfiguration?> GetNodeConfiguration();
    void SetConfiguration(NodeConfiguration configuration);
}
