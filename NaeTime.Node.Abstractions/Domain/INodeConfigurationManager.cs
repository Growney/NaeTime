using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Abstractions.Domain;

public interface INodeConfigurationManager
{
    Task<NodeConfiguration?> GetCurrentConfigurationAsync();

    event Action<NodeConfiguration> OnConfigurationChanged;
}
