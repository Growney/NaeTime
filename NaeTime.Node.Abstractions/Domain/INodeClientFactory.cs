using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Abstractions.Domain;

public interface INodeClientFactory
{
    Task<INodeClient> CreateNodeClientAsync(NodeConfiguration configuration, CancellationToken token);
}
